using Core;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryBookCatalogStore : IAsyncBookCatalogStore
{
    private readonly object _sync = new();
    private readonly Dictionary<string, BookCatalogEntry> _books =
        new(StringComparer.OrdinalIgnoreCase);

    public InMemoryBookCatalogStore()
    {
        var copyCounts = SampleData.Copies()
            .GroupBy(copy => copy.Isbn, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        foreach (Book book in SampleData.Books())
        {
            _books.Add(book.Id, new BookCatalogEntry(
                book, copyCounts.GetValueOrDefault(book.Isbn)));
        }
    }

    public Task<IReadOnlyList<BookCatalogEntry>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            IReadOnlyList<BookCatalogEntry> result = _books.Values.ToArray();
            return Task.FromResult(result);
        }
    }

    public Task<BookCatalogEntry?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_books.GetValueOrDefault(id));
        }
    }

    public Task<bool> AddAsync(Book book, int copies, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_books.TryAdd(book.Id, new BookCatalogEntry(book, copies)));
        }
    }

    public Task<bool> UpdateAsync(Book book, int copies, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (!_books.ContainsKey(book.Id))
                return Task.FromResult(false);

            _books[book.Id] = new BookCatalogEntry(book, copies);
            return Task.FromResult(true);
        }
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_books.Remove(id));
        }
    }
}
