using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryBookStore : IBookStore
{
    private readonly Dictionary<string, Book> _books;
    private readonly Dictionary<string, BookCopy> _copies;

    public InMemoryBookStore(IEnumerable<Book>? books = null, IEnumerable<BookCopy>? copies = null)
    {
        _books = (books ?? []).ToDictionary(book => book.Id, StringComparer.OrdinalIgnoreCase);
        _copies = (copies ?? []).ToDictionary(copy => copy.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Book> ListBooks() => _books.Values.ToList();

    public Book? GetBookById(string id) => _books.GetValueOrDefault(id);

    public void AddBook(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        if (_books.ContainsKey(book.Id))
            throw new InvalidOperationException($"Книга з ID '{book.Id}' уже існує.");
        _books.Add(book.Id, book);
    }

    public void UpdateBook(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        _books[book.Id] = book;
    }

    public IReadOnlyList<BookCopy> ListCopies() => _copies.Values.ToList();

    public BookCopy? GetCopyById(string id) => _copies.GetValueOrDefault(id);

    public void AddCopy(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        if (_copies.ContainsKey(copy.Id))
            throw new InvalidOperationException($"Примірник з ID '{copy.Id}' уже існує.");
        _copies.Add(copy.Id, copy);
    }

    public void UpdateCopy(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        _copies[copy.Id] = copy;
    }
}
