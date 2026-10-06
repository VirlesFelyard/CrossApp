using Core.Dto;

namespace Core.Domain;

public sealed class Book
{
    private readonly string _id;
    private readonly string _isbn;
    private readonly string _title;
    private readonly int _year;
    private readonly string? _author;

    public string Id => _id;
    public string Isbn => _isbn;
    public string Title => _title;
    public int Year => _year;
    public string? Author => _author;

    private Book(
        string id,
        string isbn,
        string title,
        int year,
        string? author)
    {
        _id = id;
        _isbn = isbn;
        _title = title;
        _year = year;
        _author = author;
    }

    public static Book Create(
        string id,
        string isbn,
        string title,
        int year,
        string? author = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор книги не може бути порожнім",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException(
                "ISBN книги не може бути порожнім",
                nameof(isbn));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Назва книги не може бути порожньою",
                nameof(title));
        }

        if (year < 1450 || year > DateTime.Now.Year)
        {
            throw new ArgumentOutOfRangeException(
                nameof(year),
                year,
                "Рік книги знаходиться поза допустимими межами");
        }

        return new Book(
            id.Trim(),
            isbn.Trim(),
            title.Trim(),
            year,
            string.IsNullOrWhiteSpace(author)
                ? null
                : author.Trim());
    }

    public BookDto ToDto()
    {
        return new BookDto(
            Id,
            Isbn,
            Title,
            Year,
            Author);
    }

    public static Book FromDto(BookDto dto)
    {
        return Create(
            dto.Id,
            dto.Isbn,
            dto.Title,
            dto.Year,
            dto.Author);
    }

    public override string ToString()
    {
        return $"{Id} | {Isbn} | {Title} | {Year} | " +
               $"{Author ?? "невідомий автор"}";
    }
}