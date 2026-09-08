using System.Runtime.InteropServices;
using System.Text.Json;

var systemInfo = new
{
    student = "Срогий Олександр, група ФЕІ-36",
    osDescription = RuntimeInformation.OSDescription,
    osVersion = Environment.OSVersion.ToString(),
    architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    dotnetVersion = Environment.Version.ToString(),
    runtime = RuntimeInformation.FrameworkDescription,
    applicationDirectory = AppContext.BaseDirectory,
    currentDirectory = Environment.CurrentDirectory,
    domain = "Бібліотека",
    entities = new[] { "Book", "BookCopy", "Reader", "Loan" }
};

if (args.Contains("--json"))
{
    Console.WriteLine(JsonSerializer.Serialize(systemInfo));
}
else
{
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine($"Студент: {systemInfo.student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription)   : {systemInfo.osDescription}");
    Console.WriteLine($"ОС (Environment)     : {systemInfo.osVersion}");
    Console.WriteLine($"Архітектура процесу  : {systemInfo.architecture}");
    Console.WriteLine($"Версія .NET (CLR)    : {systemInfo.dotnetVersion}");
    Console.WriteLine($"Runtime              : {systemInfo.runtime}");
    Console.WriteLine($"Каталог застосунку   : {systemInfo.applicationDirectory}");
    Console.WriteLine($"Поточний каталог     : {systemInfo.currentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область: Бібліотека (видання, примірники, читачі)");
}
