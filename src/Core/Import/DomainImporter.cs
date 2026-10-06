using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class DomainImporter
{
    public static ImportResult<Book> ToDomain(
        ImportResult<BookDto> source)
    {
        var items = new List<Book>();
        var errors = new List<string>();

        foreach (string error in source.Errors)
        {
            errors.Add(error);
        }

        foreach (BookDto dto in source.Items)
        {
            try
            {
                Book book = Book.FromDto(dto);

                items.Add(book);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                errors.Add(
                    $"ID '{dto.Id}': " +
                    $"{ex.GetType().Name} — {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                errors.Add(
                    $"ID '{dto.Id}': " +
                    $"{ex.GetType().Name} — {ex.Message}");
            }
        }

        int total = source.Total;
        int accepted = items.Count;
        int skipped = total - accepted;

        return new ImportResult<Book>(
            items,
            errors,
            total,
            accepted,
            skipped);
    }
}