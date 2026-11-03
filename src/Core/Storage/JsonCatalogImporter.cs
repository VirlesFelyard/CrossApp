using Core;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async IAsyncEnumerable<Book> LoadAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default,
        IProgress<int>? progress = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ct.ThrowIfCancellationRequested();
        progress?.Report(0);

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        int index = 0;
        int lastProgress = 0;

        await foreach (BookDto? dto in JsonSerializer.DeserializeAsyncEnumerable<BookDto>(
                           stream,
                           Options,
                           ct))
        {
            ct.ThrowIfCancellationRequested();
            index++;

            if (dto is null)
            {
                throw new DomainException(
                    $"Запис {index}: порожній запис.");
            }

            Book book;

            try
            {
                book = Book.FromDto(dto);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException(
                    $"Запис {index}: {ex.Message}",
                    ex);
            }

            if (stream.Length > 0)
            {
                int currentProgress = (int)Math.Min(
                    99,
                    stream.Position * 100L / stream.Length);

                if (currentProgress > lastProgress)
                {
                    lastProgress = currentProgress;
                    progress?.Report(currentProgress);
                }
            }

            yield return book;
        }

        progress?.Report(100);
    }
}
