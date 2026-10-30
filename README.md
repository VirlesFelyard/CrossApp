# CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan.

Призначення: облік примірників книг, читачів, їх видачі та повернення.

## Імпорт каталогу та тестування

Запуск імпорту каталогу з JSON-файлу:

dotnet run --project src/Cli -f net8.0 -- --file data/sample.json

Запуск модульних тестів:

dotnet test CrossApp.sln

Для перевірки скасування потрібно запустити імпорт великого JSON-файлу:

dotnet run --project src/Cli -f net8.0 -- --file data/large.json

Під час виконання натиснути Ctrl+C. Програма повинна вивести: Імпорт скасовано.

## Порівняння читабельності синтаксису запитів

Звіт BooksByPeriodQuery записано в синтаксисі запитів (from ... group ... into ...). Для групування він читається компактніше, бо ключ групи та агрегати зібрані в одному блоці. Методний синтаксис (GroupBy(...).Select(...)) краще підходить для довгих ланцюжків Where → OrderBy → ThenBy → Take, де порядок операцій читається зліва направо.

## Відкладене виконання

Запит `source.Where(...)` у `ReportService.DeferredExecutionDemo` лише описується в момент оголошення, а фактично виконується під час кожного виклику `Count()` і `ToList()`. Перший `Count()` порахував прострочені видачі до змін джерела, потім до `source` додано ще одну прострочену видачу, тому другий `Count()` повторно пройшов уже змінений список і дав число на одиницю більше. `ToList()` зафіксував результат у `snapshot`, тому після `source.Clear()` у знімку лишилась та сама кількість, що була на момент виклику `ToList()`.

## Інваріанти доменної моделі

- `Book`: ID, ISBN і назва не можуть бути порожніми; рік видання має бути від 1450 до поточного року.
- `BookCopy`: ID та ISBN не можуть бути порожніми; виданий примірник не можна видати повторно, а невиданий — повернути.
- `Loan`: ID видачі, ID примірника та ID читача не можуть бути порожніми; дата видачі повинна бути задана; дата повернення не може бути раніше дати видачі; закриту видачу не можна закрити повторно.
- `LibraryService`: один читач може мати не більше 5 одночасно відкритих видач.
- `Order`: замовлення починається у стані `Draft`; дозволені переходи `Draft → Confirmed` або `Draft → Cancelled`; після цього змінювати стан не можна.

## Доменні події

`LendingService` оголошує `event EventHandler<LibraryChangedEventArgs>? Changed`
і піднімає його лише після успішної зміни стану. Події: `BookAdded`,
`CopyIssued`, `CopyReturned`; дані: `CopyId`, `ReaderId`, `DueDate`.

## Сховище

- Інтерфейс: `IBookStore`
- Реалізації: `InMemoryBookStore`, `FileBookStore`
- Сервіс: `LendingService`

## Структура solution

```
CrossApp/
├── CrossApp.sln
├── README.md
├── data/
│   ├── mixed.csv
│   ├── sample.csv
│   ├── sample.json
│   ├── bad.json
│   └── large.json
├── tests/
│   └── Core.Tests/
│       ├── Core.Tests.csproj
│       ├── CatalogServiceTests.cs
│       ├── ImportTests.cs
│       ├── ReportServiceTests.cs
│       └── TestData.cs
└── src/
    ├── Cli/
    │   ├── Cli.csproj
    │   ├── Program.cs
    │   └── Notifications/
    │       ├── ConsoleSink.cs
    │       └── FileSink.cs
    └── Core/
        ├── Core.csproj
        ├── EnvironmentInfo.cs
        ├── SampleData.cs
        ├── DomainException.cs
        ├── Abstractions/
        │   ├── ICatalogImporter.cs
        │   ├── IBookStore.cs
        │   └── INotificationSink.cs
        ├── Domain/
        │   ├── Book.cs
        │   ├── BookCopy.cs
        │   ├── LibraryService.cs
        │   ├── Loan.cs
        │   ├── Order.cs
        │   └── OrderStatus.cs
        ├── Dto/
        │   ├── BookDto.cs
        │   ├── BookCopyDto.cs
        │   ├── ImportResult.cs
        │   ├── LibraryItemDto.cs
        │   ├── LoanDto.cs
        │   └── ReaderDto.cs
        ├── Events/
        │   ├── LibraryChangeKind.cs
        │   └── LibraryChangeKindEventArgs.cs
        ├── Import/
        │   ├── BookCsvImporter.cs
        │   ├── BookJsonImporter.cs
        │   ├── DomainImporter.cs
        │   └── MixedCsvImporter.cs
        ├── Reports/
        │   ├── BenchmarkResult.cs
        │   ├── BookLoanLine.cs
        │   ├── EnumerableExtensions.cs
        │   ├── GroupSummary.cs
        │   ├── OverdueLine.cs
        │   ├── ReaderLoanLine.cs
        │   ├── ReportBenchmark.cs
        │   └── ReportService.cs
        ├── Services/
        │   └── LendingService.cs
        └── Storage/
            ├── CachingBookStore.cs
            ├── FileBookStore.cs
            ├── InMemoryBookStore.cs
            ├── JsonCatalogImporter.cs
            └── StoreFactory.cs
```

## Формат CSV

Основний формат вхідного CSV-файлу:

```
id;isbn;title;year;author
B-001;978-617-000-001;Кобзар;1840;Тарас Шевченко
B-002;978-617-000-002;Захар Беркут;1883;Іван Франко
```

## Домовленість про каталоги в Core (на весь семестр)

```
Core/Dto/     – record-типи формату даних (тиждень 3): ProductDto / BookDto / OrderDto
Core/Domain/  – сутності з поведінкою та інваріантами (тиждень 4)
Core/Storage/ – реалізації сховищ (тиждень 5)
```

## Команди

```
dotnet build
dotnet run --project src/Cli

dotnet publish src/Cli -c Release -r win-x64 --self-contained true
dotnet publish src/Cli -c Release -r win-x64 --self-contained false
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

## Порівняння режимів публікації

| RID       | Режим               | Розмір publish | Потрібен runtime |
|-----------|---------------------|----------------|------------------|
| win-x64   | self-contained      | 70.6 МБ        | ні               |
| win-x64   | framework-dependent | 184 КБ         | так (.NET 8)     |
| linux-x64 | self-contained      | 70.5 МБ        | ні               |

**Різниця між режимами:** self-contained включає .NET Runtime всередину каталогу publish, тому застосунок запускається на машині без встановленого .NET, але займає десятки мегабайт. Framework-dependent містить лише код застосунку та його залежності — каталог крихітний, але на машині користувача має бути встановлений .NET Runtime відповідної версії.
