using Cli.Configuration;
using Core;
using Core.Storage;

namespace Cli.Commands;

public static class ApplicationCommands
{
    public static bool TryRun(string[] args)
    {
        if (args.Length == 0 ||
            !new[] { "export", "import", "config" }
                .Contains(args[0], StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            ConfigurationContext settings = ConfigurationLoader.Load(args);
            string file = settings.DataFilePath;
            string? apiKey = settings.Configuration["Integration:ApiKey"];

            Console.WriteLine($"Профіль       : {settings.ProfileName}");
            Console.WriteLine($"Файл даних    : {file}");
            Console.WriteLine(string.IsNullOrEmpty(apiKey)
                ? "ApiKey        : не заданий"
                : $"ApiKey        : присутній, довжина {apiKey.Length}");

            if (settings.Options.Logging.MinimumLevel.Equals("Debug", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine("[Debug] Налаштування каталогу завантажено; режим детального журналювання.");

            switch (args[0].ToLowerInvariant())
            {
                case "config":
                    Console.WriteLine($"Мін. рівень логу : {settings.Options.Logging.MinimumLevel}");
                    Console.WriteLine($"Термін видачі    : {settings.Options.Library.LoanDays} днів");
                    break;

                case "export":
                    var destination = new FileBookStore(file);
                    destination.ReplaceAll(SampleData.Books(), SampleData.Copies());
                    Console.WriteLine($"Експортовано книг: {destination.ListBooks().Count}");
                    Console.WriteLine($"Експортовано примірників: {destination.ListCopies().Count}");
                    break;

                case "import":
                    if (!File.Exists(file))
                    {
                        Console.Error.WriteLine(
                            "Помилка: файл каталогу не знайдено. Спочатку виконайте export.");
                        Environment.ExitCode = 1;
                        break;
                    }

                    var source = new FileBookStore(file);
                    var books = source.ListBooks();
                    var copies = source.ListCopies();
                    Console.WriteLine($"Імпортовано книг: {books.Count}");
                    Console.WriteLine($"Імпортовано примірників: {copies.Count}");
                    Console.WriteLine($"Приклад: {books.FirstOrDefault()?.Title ?? "каталог порожній"}");
                    break;
            }
        }
        catch (InvalidDataException ex)
        {
            Console.Error.WriteLine($"Помилка лабораторної №9: {ex.Message}");
            Environment.ExitCode = 1;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
                                         ArgumentException or InvalidOperationException or
                                         FormatException)
        {
            Console.Error.WriteLine($"Помилка лабораторної №9: {ex.Message}");
            Environment.ExitCode = 1;
        }

        return true;
    }
}
