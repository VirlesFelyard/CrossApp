using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

var systemInfo = new
{
    student = "Срогий Олександр, група ФЕІ-36",
    osDescription = report.OsDescription,
    frameworkDescription = report.FrameworkDescription,
    processArchitecture = report.ProcessArchitecture,
    detectedRid = report.DetectedRid,
    reportedRid = report.ReportedRid,
    baseDirectory = report.BaseDirectory,
    buildNote = EnvironmentInfo.BuildNote,
    domain = "Бібліотека",
    entities = new[] { "Book", "BookCopy", "Reader", "Loan" }
};

if (args.Contains("--json"))
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Console.WriteLine(
        JsonSerializer.Serialize(systemInfo, options));
}
else if (args.Contains("--mixed"))
{
    string path = args
        .FirstOrDefault(arg => !arg.StartsWith("--"))
        ?? Path.Combine("data", "mixed.csv");

    if (!File.Exists(path))
    {
        Console.WriteLine($"Помилка: файл не знайдено: {Path.GetFullPath(path)}");

        return;
    }

    var items = MixedCsvImporter.Load(path);

    Console.WriteLine($"Різнорідний файл: {path}");
    Console.WriteLine($"Завантажено записів: {items.Count}");
    Console.WriteLine();

    foreach (var item in items)
    {
        switch (item)
        {
            case BookDto book:
                Console.WriteLine(
                    $"Book   | {book.Id} | {book.Isbn} | " +
                    $"{book.Title} | {book.Year}");
                break;

            case ReaderDto reader:
                Console.WriteLine(
                    $"Reader | {reader.Id} | {reader.Name} | " +
                    $"{reader.Email}");
                break;
        }
    }
}
else
{
    Console.WriteLine(
        "CrossApp – практикум з крос-платформного програмування");

    Console.WriteLine($"Студент: {systemInfo.student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription)   : {report.OsDescription}");
    Console.WriteLine($"Runtime              : {report.FrameworkDescription}");
    Console.WriteLine($"Архітектура процесу  : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (визначено)      : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET)       : {report.ReportedRid}");
    Console.WriteLine($"Каталог застосунку   : {report.BaseDirectory}");
    Console.WriteLine($"Збірка Core          : {EnvironmentInfo.BuildNote}");

    Console.WriteLine(new string('-', 52));

    Console.WriteLine(
        "Предметна область: Бібліотека " +
        "(видання, примірники, читачі)");

    Console.WriteLine();

    string path = args
        .FirstOrDefault(arg => !arg.StartsWith("--"))
        ?? Path.Combine("data", "sample.csv");

    Console.WriteLine($"Файл імпорту: {path}");
    Console.WriteLine();

    if (!File.Exists(path))
    {
        Console.WriteLine(
            $"Помилка: файл не знайдено: {Path.GetFullPath(path)}");

        return;
    }

    var result = Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".csv" => BookCsvImporter.Load(path),

        ".json" => BookJsonImporter.Load(path),

        _ => new ImportResult<BookDto>(
            [],
            [
                $"Непідтримуване розширення файлу: " +
                $"{Path.GetExtension(path)}"
            ],
            0,
            0,
            0)
    };

    Console.WriteLine(
        $"Завантажено записів: {result.Items.Count}");

    Console.WriteLine(result.Statistics);
    Console.WriteLine();

    if (result.Items.Count > 0)
    {
        Console.WriteLine("Перші записи:");

        foreach (BookDto book in result.Items.Take(5))
        {
            Console.WriteLine(
                $"{book.Id} | " +
                $"{book.Isbn} | " +
                $"{book.Title} | " +
                $"{book.Year} | " +
                $"Автор: {book.Author ?? "невідомий"}");
        }
    }

    if (result.Errors.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine(
            $"Помилки ({result.Errors.Count}):");

        foreach (string error in result.Errors)
        {
            Console.WriteLine($"- {error}");
        }
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("Помилок не знайдено.");
    }
}
