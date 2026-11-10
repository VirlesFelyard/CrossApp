namespace Core.Configuration;

public sealed class AppOptions
{
    public string EnvironmentName { get; set; } = "Development";
    public StorageOptions Storage { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
    public LibraryOptions Library { get; set; } = new();
}

public sealed class StorageOptions
{
    public string DataPath { get; set; } = "data";
    public string FileName { get; set; } = "library.json";

    public string ResolveFullPath(string baseDir)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDir);
        ArgumentException.ThrowIfNullOrWhiteSpace(DataPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(FileName);

        if (Path.IsPathRooted(FileName) ||
            FileName != Path.GetFileName(FileName) ||
            FileName is "." or "..")
        {
            throw new ArgumentException(
                "Storage:FileName має бути лише назвою файлу, без шляху.",
                nameof(FileName));
        }

        string root = Path.IsPathRooted(DataPath)
            ? DataPath
            : Path.Combine(baseDir, DataPath);

        string fullDirectory = Path.GetFullPath(root);
        Directory.CreateDirectory(fullDirectory);
        return Path.GetFullPath(Path.Combine(fullDirectory, FileName));
    }
}

public sealed class LoggingOptions
{
    public string MinimumLevel { get; set; } = "Information";
}

public sealed class LibraryOptions
{
    public int LoanDays { get; set; } = 14;
}
