using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    Task<IReadOnlyList<Book>> LoadAsync(
        string path,
        CancellationToken ct = default);
}
