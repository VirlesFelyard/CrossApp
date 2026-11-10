using System.Reflection;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Cli.Configuration;

public static class ConfigurationLoader
{
    public static ConfigurationContext Load(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        string? requestedEnvironment = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT");
        string environment = string.IsNullOrWhiteSpace(requestedEnvironment)
            ? "Development"
            : requestedEnvironment.Trim();

        if (environment.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
            throw new ArgumentException("CROSSAPP_ENVIRONMENT містить недопустимі символи.");

        string[] overrides = args
            .Where(arg => arg.StartsWith("--", StringComparison.Ordinal) &&
                          arg.Contains('=') &&
                          (arg.StartsWith("--Storage:", StringComparison.OrdinalIgnoreCase) ||
                           arg.StartsWith("--Logging:", StringComparison.OrdinalIgnoreCase) ||
                           arg.StartsWith("--Library:", StringComparison.OrdinalIgnoreCase) ||
                           arg.StartsWith("--EnvironmentName=", StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables(prefix: "CROSSAPP_")
            .AddCommandLine(overrides)
            .Build();

        AppOptions options = config.Get<AppOptions>() ?? new AppOptions();
        string? simplePath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");

        var storage = new StorageOptions
        {
            DataPath = string.IsNullOrWhiteSpace(simplePath)
                ? options.Storage.DataPath
                : simplePath,
            FileName = options.Storage.FileName
        };

        if (options.Library.LoanDays <= 0)
            throw new ArgumentException("Library:LoanDays повинно бути додатним числом.");

        return new ConfigurationContext(
            config,
            options,
            environment,
            storage.ResolveFullPath(AppContext.BaseDirectory));
    }
}

public sealed record ConfigurationContext(
    IConfiguration Configuration,
    AppOptions Options,
    string ProfileName,
    string DataFilePath);
