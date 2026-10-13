using Core;
using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    public static IBookStore Create(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        IBookStore store;
        if (args.Contains("--file", StringComparer.OrdinalIgnoreCase))
        {
            string path = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
            store = new FileBookStore(path);
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
