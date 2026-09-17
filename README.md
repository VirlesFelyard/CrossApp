# CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Бібліотека. Сутності: Book, BookCopy, Reader, Loan.

Призначення: облік примірників книг, читачів, їх видачі та повернення.

## Структура solution

```
CrossApp/
├── CrossApp.sln
├── global.json
├── README.md
├── .gitignore
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   └── EnvironmentInfo.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```

## Команди

```
dotnet build
dotnet run --project src/Cli

dotnet publish src/Cli -c Release -r win-x64 --self-contained true
dotnet publish src/Cli -c Release -r win-x64 --self-contained false
```

## Порівняння режимів публікації

| RID     | Режим               | Розмір publish | Потрібен runtime |
|---------|---------------------|----------------|------------------|
| win-x64 | self-contained      | 70.6 МБ        | ні               |
| win-x64 | framework-dependent | 184 КБ         | так (.NET 8)     |