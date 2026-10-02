using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core;
using Core.Domain;
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
        Console.WriteLine(
            $"Помилка: файл не знайдено: {Path.GetFullPath(path)}");

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
                    $"{book.Title} | {book.Year} | {book.Author}");
                break;

            case ReaderDto reader:
                Console.WriteLine(
                    $"Reader | {reader.Id} | {reader.Name} | " +
                    $"{reader.Email}");
                break;
        }
    }
}
else if (args.Contains("--domain"))
{
    RunDomainDemo();
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
}

static void RunDomainDemo()
{
    Console.WriteLine("Успішні операції");

    BookCopy copy = BookCopy.Create(
        "C-001",
        "978-617-000-001");

    Console.WriteLine($"Створено: {copy}");

    copy.Issue();

    Console.WriteLine($"Після Issue(): {copy}");

    Loan loan = Loan.Open(
        "L-001",
        "C-001",
        "R-001",
        new DateTime(2026, 10, 1));

    Console.WriteLine($"Створено: {loan}");

    loan.Close(
        new DateTime(2026, 10, 10));

    Console.WriteLine($"Після Close(): {loan}");

    copy.Return();

    Console.WriteLine($"Після Return(): {copy}");

    Console.WriteLine();
    Console.WriteLine("Порушення інваріантів");

    TryDo(
        "Порожній ID",
        () => BookCopy.Create(
            "",
            "978-617-000-002"));

    TryDo(
        "Порожній ISBN",
        () => BookCopy.Create(
            "C-002",
            " "));

    BookCopy issuedCopy = BookCopy.Create(
        "C-003",
        "978-617-000-003");

    issuedCopy.Issue();

    TryDo(
        "Повторна видача примірника",
        () => issuedCopy.Issue());

    TryDo(
        "Повернення невиданого примірника",
        () => copy.Return());

    TryDo(
        "Порожній ReaderId",
        () => Loan.Open(
            "L-002",
            "C-003",
            "",
            new DateTime(2026, 10, 1)));

    Loan invalidDateLoan = Loan.Open(
        "L-003",
        "C-003",
        "R-003",
        new DateTime(2026, 10, 10));

    TryDo(
        "Дата повернення раніше дати видачі",
        () => invalidDateLoan.Close(
            new DateTime(2026, 10, 1)));

    TryDo(
        "Повторне закриття видачі",
        () => loan.Close(
            new DateTime(2026, 10, 15)));

    Console.WriteLine();
    Console.WriteLine("DTO mapping");

    BookCopyDto copyDto = issuedCopy.ToDto();

    Console.WriteLine(
        $"BookCopyDto: {copyDto.Id} | " +
        $"{copyDto.Isbn} | " +
        $"Виданий: {copyDto.IsIssued}");

    BookCopy restoredCopy =
        BookCopy.FromDto(copyDto);

    Console.WriteLine(
        $"BookCopy після FromDto(): {restoredCopy}");

    LoanDto loanDto = loan.ToDto();

    Console.WriteLine(
        $"LoanDto: {loanDto.Id} | " +
        $"{loanDto.CopyId} | " +
        $"{loanDto.ReaderId} | " +
        $"{loanDto.IssuedOn:d} | " +
        $"{loanDto.ReturnedOn:d}");

    Loan restoredLoan =
        Loan.FromDto(loanDto);

    Console.WriteLine(
        $"Loan після FromDto(): {restoredLoan}");
}

static void TryDo(
    string title,
    Action action)
{
    try
    {
        action();

        Console.WriteLine(
            $"{title}: ВИНЯТОК НЕ ВИНИК — інваріант не спрацював!");
    }
    catch (ArgumentOutOfRangeException ex)
    {
        Console.WriteLine(
            $"{title}: {ex.GetType().Name} — {ex.Message}");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine(
            $"{title}: {ex.GetType().Name} — {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine(
            $"{title}: {ex.GetType().Name} — {ex.Message}");
    }
}