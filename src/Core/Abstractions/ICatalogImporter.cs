using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    IAsyncEnumerable<Book> LoadAsync(
        string path,
        CancellationToken ct = default,
        IProgress<int>? progress = null);
}
