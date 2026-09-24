# CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan.

Призначення: облік примірників книг, читачів, їх видачі та повернення.

## Структура solution

```
## Структура solution

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
    │   ├── Dto/
    │   │   ├── BookDto.cs
    │   │   └── ImportResult.cs
    │   └── Import/
    │       └── BookCsvImporter.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
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