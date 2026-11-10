using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Import;
using Core.Services;
using Core.Storage;
using Core.Events;
using Core.Reports;
using Cli.Notifications;
using Cli.Commands;
using Cli.Configuration;

Console.OutputEncoding = Encoding.UTF8;

if (ApplicationCommands.TryRun(args))
    return;

string? reportCommand = args.FirstOrDefault(arg => !arg.StartsWith("--"));

if (reportCommand is not null && reportCommand.StartsWith("report-", StringComparison.Ordinal))
{
    RunReport(reportCommand);
    return;
}

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
    string path = ResolveDemoDataPath(
        args.FirstOrDefault(arg => !arg.StartsWith("--")),
        "mixed.csv");

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
else if (args.Contains("--storage") || args.Contains("--cache") ||
         args.Contains("--search") || args.Length == 0)
{
    RunStorageDemo(args);
}
else if (args.Contains("--file"))
{
    await RunAsyncImport(args);
}
else if (args.Contains("--domain"))
{
    RunDomainDemo();
}
else if (args.Contains("--domain1"))
{
    RunImportToDomainDemo();
}
else if (args.Contains("--domain2"))
{
    RunCrossEntityInvariantDemo();
}
else if (args.Contains("--domain3"))
{
    RunOrderStatusDemo();
}
else if (args.Contains("--events"))
{
    RunEventsDemo();
}
else if (args.Contains("--events1"))
{
    RunSinkDemo();
}
else if (args.Contains("--events2"))
{
    RunImportReportDemo();
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


static string ResolveDemoDataPath(string? relativeOrAbsolute, string defaultFileName)
{
    string path = relativeOrAbsolute ??
        Path.Combine(AppContext.BaseDirectory, "data", defaultFileName);

    return Path.GetFullPath(Path.IsPathRooted(path)
        ? path
        : Path.Combine(AppContext.BaseDirectory, path));
}

static async Task RunAsyncImport(string[] args)
{
    string dataPath = ResolveDemoDataPath(
        args.FirstOrDefault(arg => !arg.StartsWith("--")),
        "sample.json");

    var service = new LendingService(new InMemoryBookStore());
    using var userCts = new CancellationTokenSource();
    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

    ConsoleCancelEventHandler handler = (_, e) =>
    {
        e.Cancel = true;
        userCts.Cancel();
    };

    Console.CancelKeyPress += handler;
    IProgress<int> progress = new ConsoleImportProgress();

    try
    {
        int added = await service.ImportAsync(
            new JsonCatalogImporter(),
            dataPath,
            userCts.Token,
            timeoutCts.Token,
            progress);

        Console.WriteLine();
        Console.WriteLine($"Імпортовано записів: {added}");
    }
    catch (OperationCanceledException)
        when (timeoutCts.IsCancellationRequested &&
              !userCts.IsCancellationRequested)
    {
        Console.WriteLine();
        Console.WriteLine("Імпорт скасовано через тайм-аут.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine();
        Console.WriteLine("Імпорт скасовано.");
    }
    catch (DomainException ex)
    {
        Console.WriteLine();
        Console.WriteLine($"Дані відхилено: {ex.Message}");
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine();
        Console.WriteLine($"Файл не знайдено: {dataPath}");
    }
    finally
    {
        Console.CancelKeyPress -= handler;
    }
}

static void RunReport(string command)
{
    var books = SampleData.Books();
    var copies = SampleData.Copies();
    var readers = SampleData.Readers();
    var loans = SampleData.Loans();
    DateTime today = DateTime.Today;

    switch (command)
    {
        case "report-top":
            var top = ReportService.TopReaders(readers, loans, 5);
            Console.WriteLine("Топ-5 читачів за кількістю видач");
            Console.WriteLine($"Усього читачів: {readers.Count}, усього видач: {loans.Count}");
            Console.WriteLine();
            Console.WriteLine($"  {"№",-3} {"ID",-8} {"Ім'я",-20} {"Видач",6}");
            Console.WriteLine($"  {new string('-', 40)}");
            int place = 1;
            foreach (var line in top)
            {
                Console.WriteLine($"  {place,-3} {line.ReaderId,-8} {line.Name,-20} {line.Loans,6}");
                place++;
            }
            Console.WriteLine();
            Console.WriteLine("Порядок: за кількістю видач (спадання), потім за ID читача");
            break;

        case "report-overdue":
            var overdue = ReportService.Overdue(loans, today);
            Console.WriteLine($"Прострочені видачі станом на {today:yyyy-MM-dd}");
            Console.WriteLine();
            Console.WriteLine($"  {"Видача",-8} {"Примірник",-10} {"Читач",-8} {"Строк",-12} {"Днів",5}");
            Console.WriteLine($"  {new string('-', 47)}");
            foreach (var line in overdue)
            {
                Console.WriteLine(
                    $"  {line.LoanId,-8} {line.CopyId,-10} {line.ReaderId,-8} " +
                    $"{line.DueDate:yyyy-MM-dd,-12} {(today - line.DueDate).Days,5}");
            }
            Console.WriteLine();
            Console.WriteLine($"Усього прострочених: {overdue.Count} з {loans.Count} видач");
            break;

        case "report-books":
            var topBooks = ReportService.TopBooks(books, copies, loans, 5);
            Console.WriteLine("Топ-5 видань за кількістю видач");
            Console.WriteLine();
            Console.WriteLine($"  {"№",-3} {"ID",-8} {"Назва",-36} {"Видач",6}");
            Console.WriteLine($"  {new string('-', 56)}");
            int rank = 1;
            foreach (var line in topBooks)
            {
                Console.WriteLine($"  {rank,-3} {line.BookId,-8} {line.Title,-36} {line.Loans,6}");
                rank++;
            }
            Console.WriteLine();
            Console.WriteLine("Порядок: за кількістю видач (спадання), потім за ID видання");
            break;

        case "report-groups":
        case "report-groups-query":
            var groups = command == "report-groups-query"
                ? ReportService.BooksByPeriodQuery(books, copies)
                : ReportService.BooksByPeriod(books, copies);
            Console.WriteLine("Видання за періодом випуску");
            Console.WriteLine();
            Console.WriteLine($"  {"Період",-16} {"Видань",8} {"Примірників",12}");
            Console.WriteLine($"  {new string('-', 38)}");
            foreach (var group in groups)
                Console.WriteLine($"  {group.Key,-16} {group.Count,8} {group.TotalCopies,12}");
            Console.WriteLine($"  {new string('-', 38)}");
            Console.WriteLine($"  {"Разом",-16} {groups.Sum(group => group.Count),8} {groups.Sum(group => group.TotalCopies),12}");
            Console.WriteLine();
            Console.WriteLine($"Груп (періодів): {groups.Count}");
            break;

        case "report-sum":
            var totals = ReportService.Totals(books, copies);
            Console.WriteLine("Підсумок за примірниками");
            Console.WriteLine();
            Console.WriteLine($"  Книг у каталозі          : {totals.Books}");
            Console.WriteLine($"  Примірників усього       : {totals.Copies}");
            Console.WriteLine($"  Середньо на книгу        : {totals.Average:F2}");
            Console.WriteLine($"  Максимум для однієї книги: {totals.Max}");
            break;

        case "report-deferred":
            var demo = ReportService.DeferredExecutionDemo(loans, today);
            Console.WriteLine("Демонстрація відкладеного виконання");
            Console.WriteLine("Запит: прострочені видачі (не повернуті, строк < сьогодні)");
            Console.WriteLine();
            Console.WriteLine($"  1. Прострочених до змін джерела   : {demo.Before}");
            Console.WriteLine($"  2. Прострочених після змін джерела: {demo.After}");
            Console.WriteLine($"  3. У знімку після Clear()         : {demo.Snapshot}");
            Console.WriteLine();
            break;

        case "report-benchmark":
            var bench = ReportBenchmark.Compare(100_000, today);
            Console.WriteLine("Порівняння ToList у середині ланцюжка (прострочені видачі за читачами)");
            Console.WriteLine();
            Console.WriteLine($"  Записів              : {bench.Records}");
            Console.WriteLine($"  Без ToList у середині: {bench.WithoutToListMs:F2} мс");
            Console.WriteLine($"  З ToList у середині  : {bench.WithToListMs:F2} мс");
            break;

        default:
            Console.WriteLine("report-top | report-overdue | report-books | report-groups | report-groups-query | report-sum | report-deferred | report-benchmark");
            break;
    }
}

static void RunStorageDemo(string[] args)
{
    string configuredFilePath = ConfigurationLoader.Load(args).DataFilePath;
    IBookStore store = StoreFactory.Create(args, configuredFilePath);
    var service = new LendingService(store);

    Console.WriteLine($"Сховище: {store.GetType().Name}");
    Console.WriteLine();

    Book book;
    if (service.AllBooks().Count == 0)
    {
        book = service.AddBook("978-617-000-100", "Нова книга", 2020, "Демо автор");
        Console.WriteLine($"Додано книгу: {book}");
    }
    else
    {
        book = service.AllBooks()[0];
        Console.WriteLine($"Знайдено наявну книгу: {book}");
    }

    Book updated = service.UpdateBook(book.Id, book.Isbn, book.Title + " (оновлено)", book.Year, book.Author);
    Console.WriteLine($"Оновлено книгу: {updated}");

    BookCopy copy;
    if (service.AllCopies().Count == 0)
    {
        copy = service.AddCopy(updated.Id);
        Console.WriteLine($"Додано примірник: {copy}");
    }
    else
    {
        copy = service.AllCopies()[0];
    }

    service.IssueCopy(copy.Id,"R-001",DateTime.Today.AddDays(14));
    Console.WriteLine($"Після видачі: {service.AllCopies().First(item => item.Id == copy.Id)}");
    service.ReturnCopy(copy.Id);
    Console.WriteLine($"Після повернення: {service.AllCopies().First(item => item.Id == copy.Id)}");

    Console.WriteLine();
    Console.WriteLine("Пошук книги за id:");
    Book? tryFound = service.FindBook(book.Id);
    Console.WriteLine(tryFound is null
        ? $"  Книгу з id={book.Id} не знайдено."
        : $"  Знайдено: {tryFound}");

    Console.WriteLine();
    Console.WriteLine("Список книг:");
    foreach (Book item in service.AllBooks())
        Console.WriteLine($"  {item}");


    if (args.Contains("--search"))
    {
        Console.WriteLine();
        Console.WriteLine("Пошук книг за предикатом: рік видання < 1900");
        IReadOnlyList<Book> found = service.SearchBooks(book => book.Year < 1900);
        foreach (Book item in found)
            Console.WriteLine($"  {item.Title} ({item.Year})");
        Console.WriteLine($"Знайдено: {found.Count}");
    }

    if (store is CachingBookStore cachingStore)
    {
        service.AllBooks();
        service.AllBooks();
        service.FindBook(book.Id);
        service.FindBook(book.Id);
        Console.WriteLine();
        Console.WriteLine("Статистика кешування:");
        Console.WriteLine($"Cache hits: {cachingStore.CacheHits}");
        Console.WriteLine($"Cache misses: {cachingStore.CacheMisses}");
    }

    Console.WriteLine();
    TryDo("Пошук неіснуючого примірника", () =>
        service.IssueCopy("COPY-MISSING", "R-002", DateTime.Today.AddDays(14)));

    if (args.Contains("--file"))
    {
        Console.WriteLine();
        Console.WriteLine($"Файл даних: {configuredFilePath}");
    }
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

static void RunImportToDomainDemo()
{
    Console.WriteLine(
        "Додаткове завдання 1");

    Console.WriteLine(
        "ImportResult<BookDto> -> ImportResult<Book>");

    Console.WriteLine();

    var source = new ImportResult<BookDto>(
        [
            new BookDto(
                "B-001",
                "978-617-001",
                "Кобзар",
                1840,
                "Тарас Шевченко"),

            new BookDto(
                "B-002",
                "",
                "Захар Беркут",
                1883,
                "Іван Франко"),

            new BookDto(
                "B-003",
                "978-617-003",
                "Тигролови",
                999,
                "Іван Багряний")
        ],
        [],
        3,
        3,
        0);

    ImportResult<Book> result = DomainImporter.ToDomain(source);

    Console.WriteLine($"Усього: {result.Total}");

    Console.WriteLine($"Прийнято: {result.Accepted}");

    Console.WriteLine($"Пропущено: {result.Skipped}");

    Console.WriteLine();

    Console.WriteLine("Створені доменні сутності:");

    foreach (Book book in result.Items)
    {
        Console.WriteLine($"  {book}");
    }

    Console.WriteLine();

    Console.WriteLine("Помилки:");

    if (result.Errors.Count == 0)
    {
        Console.WriteLine("  Помилок немає.");
    }
    else
    {
        foreach (string error in result.Errors)
        {
            Console.WriteLine($"  ! {error}");
        }
    }
}

static void RunCrossEntityInvariantDemo()
{
    Console.WriteLine("Додаткове завдання 2");
    Console.WriteLine("Перевірка максимальної кількості відкритих видач");
    Console.WriteLine();

    var service = new LibraryService();
    var loans = new List<Loan>();

    for (int i = 1; i <= 5; i++)
    {
        BookCopy copy = BookCopy.Create(
            $"C-{i:000}",
            $"978-617-000-{i:000}");

        loans.Add(service.IssueBook(
            copy,
            $"L-{i:000}",
            "R-001",
            new DateTime(2026, 10, i),
            loans));
    }

    Console.WriteLine($"Відкритих видач: {loans.Count}");

    BookCopy sixthCopy = BookCopy.Create(
        "C-006",
        "978-617-000-006");

    TryDo(
        "Шоста видача для того самого читача",
        () => service.IssueBook(
            sixthCopy,
            "L-006",
            "R-001",
            new DateTime(2026, 10, 6),
            loans));
}

static void RunOrderStatusDemo()
{
    Console.WriteLine("Додаткове завдання 3");
    Console.WriteLine("Перевірка переходів стану Order");
    Console.WriteLine();

    Order order = Order.Create("O-001");

    Console.WriteLine($"Створено: {order}");

    order.ChangeStatus(OrderStatus.Confirmed);

    Console.WriteLine($"Після Confirmed: {order}");

    TryDo(
        "Перехід Confirmed -> Cancelled",
        () => order.ChangeStatus(OrderStatus.Cancelled));

    Order cancelledOrder = Order.Create("O-002");

    cancelledOrder.ChangeStatus(OrderStatus.Cancelled);

    Console.WriteLine($"Створено: {cancelledOrder}");

    TryDo(
        "Перехід Cancelled -> Confirmed",
        () => cancelledOrder.ChangeStatus(OrderStatus.Confirmed));
}

static void RunSinkDemo()
{
    string logPath = Path.Combine(
        AppContext.BaseDirectory,
        "logs",
        "sink.log");

    INotificationSink[] sinks =
    [
        new ConsoleSink(),
        new FileSink(logPath)
    ];

    IBookStore store = new InMemoryBookStore();
    var service = new LendingService(store, sinks);

    Book book = service.AddBook(
        "978-617-000-101",
        "Основи програмування",
        2022,
        "Демо автор");

    BookCopy copy = service.AddCopy(book.Id);

    service.IssueCopy(
        copy.Id,
        "R-001",
        DateTime.Today.AddDays(14));

    service.ReturnCopy(copy.Id);

    Console.WriteLine();
    Console.WriteLine($"Журнал: {logPath}");
}

static void RunImportReportDemo()
{
    string path = ResolveDemoDataPath(null, "sample.csv");

    ImportResult<BookDto> result = BookCsvImporter.Load(
        path,
        message => Console.WriteLine($"Пропущено (Action): {message}"));

    Console.WriteLine();
    Console.WriteLine($"Усього рядків: {result.Total}");
    Console.WriteLine($"Прийнято: {result.Accepted}");
    Console.WriteLine($"Пропущено: {result.Skipped}");
}

static void RunEventsDemo()
{
    var store = new InMemoryBookStore();
    var service = new LendingService(store);

    string logPath = Path.Combine(
        AppContext.BaseDirectory,
        "logs",
        "app.log");

    Directory.CreateDirectory(
        Path.GetDirectoryName(logPath)!);

    File.WriteAllText(logPath, string.Empty);

    Console.WriteLine();

    void OnLibraryChanged(
        object? sender,
        LibraryChangedEventArgs e)
    {
        string line = e.Kind switch
        {
            LibraryChangeKind.BookAdded =>
                $"{e.At:HH:mm:ss} BookAdded | " +
                $"Додано книгу {e.BookId}",

            LibraryChangeKind.CopyIssued =>
                $"{e.At:HH:mm:ss} CopyIssued | " +
                $"Примірник {e.CopyId} видано читачу " +
                $"{e.ReaderId} до {e.DueDate:yyyy-MM-dd}",

            LibraryChangeKind.CopyReturned =>
                $"{e.At:HH:mm:ss} CopyReturned | " +
                $"Примірник {e.CopyId} повернуто. " +
                $"Читач: {e.ReaderId ?? "невідомий"}, " +
                $"строк повернення: " +
                $"{e.DueDate?.ToString("yyyy-MM-dd") ?? "невідомий"}",

            _ => throw new ArgumentOutOfRangeException(
                nameof(e.Kind))
        };

        Console.WriteLine(line);

        try
        {
            File.AppendAllText(
                logPath,
                line + Environment.NewLine);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine(
                $"Помилка запису журналу: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine(
                $"Немає доступу до журналу: {ex.Message}");
        }
    }

    service.Changed += OnLibraryChanged;

    int changes = 0;

    EventHandler<LibraryChangedEventArgs> counterHandler =
        (_, _) => changes++;

    service.Changed += counterHandler;

    DateTime dueDate = DateTime.Today.AddDays(14);

    Console.WriteLine("1. Успішні операції");

    Book book = service.AddBook(
        "978-617-000-101",
        "Основи програмування",
        2022,
        "Демо автор");

    BookCopy copy = service.AddCopy(book.Id);

    service.IssueCopy(
        copy.Id,
        "R-001",
        dueDate);

    service.ReturnCopy(copy.Id);

    Console.WriteLine();
    Console.WriteLine($"Подій отримано: {changes}");

    Console.WriteLine();
    Console.WriteLine("2. Перевірка відписки");

    int changesBeforeUnsubscribe = changes;

    service.Changed -= OnLibraryChanged;

    Console.WriteLine(
        $"Подій до відписки: {changesBeforeUnsubscribe}");

    BookCopy secondCopy = service.AddCopy(book.Id);

    service.IssueCopy(
        secondCopy.Id,
        "R-002",
        DateTime.Today.AddDays(21));

    service.ReturnCopy(secondCopy.Id);

    service.IssueCopy(
        secondCopy.Id,
        "R-003",
        DateTime.Today.AddDays(28));

    Console.WriteLine(
        "Консольний обробник відписаний.");

    Console.WriteLine(
        $"Подій отримано лічильником: {changes}");

    Console.WriteLine(
        $"Приріст після відписки: {changes - changesBeforeUnsubscribe}");


    Console.WriteLine();
    Console.WriteLine("3. Перевірка помилкової операції");

    int changesBeforeError = changes;

    try
    {
        service.IssueCopy(
            "COPY-MISSING",
            "R-999",
            dueDate);

        Console.WriteLine(
            "Помилка: очікуваний виняток не виник.");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine(
            $"Очікувана помилка: {ex.Message}");
    }

    Console.WriteLine(
        $"Подій до помилки: {changesBeforeError}");

    Console.WriteLine(
        $"Подій після помилки: {changes}");

    Console.WriteLine();
    Console.WriteLine("4. Пошук через Func");

    IReadOnlyList<BookCopy> availableCopies =
        service.FindAll(copy => !copy.IsIssued);

    foreach (BookCopy availableCopy in availableCopies)
    {
        Console.WriteLine(
            $"Доступний примірник: {availableCopy.Id}");
    }

    Console.WriteLine(
        $"Знайдено доступних примірників: " + $"{availableCopies.Count}");

    Console.WriteLine();
    Console.WriteLine($"Файл журналу: {logPath}");
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
    catch (DomainException ex)
    {
        Console.WriteLine(
            $"{title}: {ex.GetType().Name} — {ex.Message}");
    }
}

sealed class ConsoleImportProgress : IProgress<int>
{
    private int _lastPercent = -1;

    public void Report(int value)
    {
        int percent = Math.Clamp(value, 0, 100);

        if (percent == _lastPercent)
        {
            return;
        }

        _lastPercent = percent;
        Console.Write((char)13);
        Console.Write($"Імпорт: {percent,3}%");
    }
}
