# CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan.

Призначення: облік примірників книг, читачів, їх видачі та повернення.

## Інваріанти доменної моделі

- `Book`: ID, ISBN і назва не можуть бути порожніми; рік видання має бути від 1450 до поточного року.
- `BookCopy`: ID та ISBN не можуть бути порожніми; виданий примірник не можна видати повторно, а невиданий — повернути.
- `Loan`: ID видачі, ID примірника та ID читача не можуть бути порожніми; дата видачі повинна бути задана; дата повернення не може бути раніше дати видачі; закриту видачу не можна закрити повторно.
- `LibraryService`: один читач може мати не більше 5 одночасно відкритих видач.
- `Order`: замовлення починається у стані `Draft`; дозволені переходи `Draft → Confirmed` або `Draft → Cancelled`; після цього змінювати стан не можна.

## Структура solution

```
CrossApp/
├── CrossApp.sln
├── global.json
├── README.md
├── .gitignore
├── data/
│   └── sample.csv
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   ├── Domain/
    │   │   ├── Book.cs
    │   │   ├── BookCopy.cs
    │   │   ├── Loan.cs
    │   │   ├── LibraryService.cs
    │   │   ├── Order.cs
    │   │   └── OrderStatus.cs
    │   ├── Dto/
    │   │   ├── BookDto.cs
    │   │   ├── BookCopyDto.cs
    │   │   ├── LoanDto.cs
    │   │   ├── ReaderDto.cs
    │   │   └── ImportResult.cs
    │   └── Import/
    │       ├── BookCsvImporter.cs
    │       ├── BookJsonImporter.cs
    │       ├── MixedCsvImporter.cs
    │       └── DomainImporter.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
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