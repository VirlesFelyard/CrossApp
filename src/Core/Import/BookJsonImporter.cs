using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class BookJsonImporter
{
    public static ImportResult<BookDto> Load(string path)
    {
        var items = new List<BookDto>();
        var errors = new List<string>();

        string json = File.ReadAllText(path);

        using JsonDocument document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return new ImportResult<BookDto>(
                [],
                ["JSON повинен містити масив об'єктів"],
                0,
                0,
                0);
        }

        int total = document.RootElement.GetArrayLength();

        int lineNumber = 0;

        foreach (JsonElement element in document.RootElement.EnumerateArray())
        {
            lineNumber++;

            try
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    errors.Add(
                        $"елемент {lineNumber}: очікую JSON-об'єкт");

                    continue;
                }

                BookDto? book = element.Deserialize<BookDto>(
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (book is null)
                {
                    errors.Add(
                        $"елемент {lineNumber}: не вдалося десеріалізувати книгу");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(book.Id))
                {
                    errors.Add(
                        $"елемент {lineNumber}: ID порожній");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(book.Isbn))
                {
                    errors.Add(
                        $"елемент {lineNumber}: ISBN порожній");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(book.Title))
                {
                    errors.Add(
                        $"елемент {lineNumber}: назва порожня");

                    continue;
                }

                if (book.Year < 1450 || book.Year > DateTime.Now.Year)
                {
                    errors.Add(
                        $"елемент {lineNumber}: рік '{book.Year}' поза допустимими межами");

                    continue;
                }

                items.Add(book);
            }
            catch (JsonException ex)
            {
                errors.Add(
                    $"елемент {lineNumber}: помилка JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                errors.Add(
                    $"елемент {lineNumber}: {ex.Message}");
            }
        }

        int accepted = items.Count;
        int skipped = total - accepted;

        return new ImportResult<BookDto>(
            items,
            errors,
            total,
            accepted,
            skipped);
    }
}