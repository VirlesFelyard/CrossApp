using Core.Domain;

namespace Core.Abstractions;

public sealed record BookCatalogEntry(Book Book, int Copies);

public interface IAsyncBookCatalogStore
{
    Task<IReadOnlyList<BookCatalogEntry>> ListAsync(CancellationToken cancellationToken);
    Task<BookCatalogEntry?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<bool> AddAsync(Book book, int copies, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Book book, int copies, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
