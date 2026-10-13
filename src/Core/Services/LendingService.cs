using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class LendingService
{
    private readonly IBookStore _store;

    public LendingService(IBookStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Book AddBook(string isbn, string title, int year, string? author = null)
    {
        var book = Book.Create(Guid.NewGuid().ToString("N")[..8], isbn, title, year, author);
        _store.AddBook(book);
        return book;
    }

    public Book UpdateBook(string id, string isbn, string title, int year, string? author = null)
    {
        if (_store.GetBookById(id) is null)
            throw new InvalidOperationException($"Книгу з ID '{id}' не знайдено.");

        var book = Book.Create(id, isbn, title, year, author);
        _store.UpdateBook(book);
        return book;
    }

    public BookCopy AddCopy(string bookId)
    {
        Book book = _store.GetBookById(bookId)
            ?? throw new InvalidOperationException($"Книгу з ID '{bookId}' не знайдено.");

        var copy = BookCopy.Create(Guid.NewGuid().ToString("N")[..8], book.Isbn);
        _store.AddCopy(copy);
        return copy;
    }

    public void IssueCopy(string copyId)
    {
        BookCopy copy = _store.GetCopyById(copyId)
            ?? throw new InvalidOperationException($"Примірник з ID '{copyId}' не знайдено.");
        copy.Issue();
        _store.UpdateCopy(copy);
    }

    public void ReturnCopy(string copyId)
    {
        BookCopy copy = _store.GetCopyById(copyId)
            ?? throw new InvalidOperationException($"Примірник з ID '{copyId}' не знайдено.");
        copy.Return();
        _store.UpdateCopy(copy);
    }

    public IReadOnlyList<Book> AllBooks() => _store.ListBooks();

    public Book? FindBook(string id) => _store.GetBookById(id);

    public IReadOnlyList<BookCopy> AllCopies() => _store.ListCopies();
}
