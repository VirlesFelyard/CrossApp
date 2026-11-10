using Core;
using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    public static IBookStore Create(string[] args, string? configuredFilePath = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        IBookStore store;
        if (args.Contains("--file", StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(configuredFilePath))
                throw new ArgumentException("Шлях до файлового сховища має надходити з конфігурації.", nameof(configuredFilePath));
            store = new FileBookStore(configuredFilePath);
        }
        else
        {
            store = new InMemoryBookStore(SampleData.Books(), SampleData.Copies());
        }

        if (args.Contains("--cache", StringComparer.OrdinalIgnoreCase))
            store = new CachingBookStore(store);

        return store;
    }
}
