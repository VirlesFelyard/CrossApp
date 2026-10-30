using Core;
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

    public async Task<IReadOnlyList<Book>> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        List<BookDto> dtos = await JsonSerializer.DeserializeAsync<List<BookDto>>(
            stream,
            Options,
            ct)
            ?? throw new DomainException(
                $"Файл {Path.GetFileName(path)} не містить списку.");

        var result = new List<Book>(dtos.Count);

        for (int i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                result.Add(Book.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                throw new DomainException(
                    $"Запис {i + 1}: {ex.Message}",
                    ex);
            }
        }

        return result;
    }
}
