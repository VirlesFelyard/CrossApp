using Core.Abstractions;

namespace Api.Contracts;

public sealed record BookDto(
    string Id,
    string Isbn,
    string Title,
    string Author,
    int Year,
    int Copies)
{
    public static BookDto From(BookCatalogEntry entry) =>
        new(entry.Book.Id, entry.Book.Isbn, entry.Book.Title,
            entry.Book.Author ?? string.Empty, entry.Book.Year, entry.Copies);
}
