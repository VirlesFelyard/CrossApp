
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class LendingService
{
    private readonly IBookStore _store;

    private readonly Dictionary<string, (string ReaderId, DateTime DueDate)>
        _activeLoans = new(StringComparer.OrdinalIgnoreCase);

    public event EventHandler<LibraryChangedEventArgs>? Changed;

    public LendingService(IBookStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    private void OnChanged(
        LibraryChangeKind kind,
        string? bookId = null,
        string? copyId = null,
        string? readerId = null,
        DateTime? dueDate = null)
    {
        Changed?.Invoke(
            this,
            new LibraryChangedEventArgs(
                kind,
                bookId,
                copyId,
                readerId,
                dueDate));
    }

    public Book AddBook(
        string isbn,
        string title,
        int year,
        string? author = null)
    {
        var book = Book.Create(
            Guid.NewGuid().ToString("N")[..8],
            isbn,
            title,
            year,
            author);

        _store.AddBook(book);

        OnChanged(
            LibraryChangeKind.BookAdded,
            bookId: book.Id);

        return book;
    }

    public Book UpdateBook(
        string id,
        string isbn,
        string title,
        int year,
        string? author = null)
    {
        if (_store.GetBookById(id) is null)
        {
            throw new InvalidOperationException(
                $"Книгу з ID '{id}' не знайдено.");
        }

        var book = Book.Create(id, isbn, title, year, author);

        _store.UpdateBook(book);

        return book;
    }

    public BookCopy AddCopy(string bookId)
    {
        Book book = _store.GetBookById(bookId)
            ?? throw new InvalidOperationException(
                $"Книгу з ID '{bookId}' не знайдено.");

        var copy = BookCopy.Create(
            Guid.NewGuid().ToString("N")[..8],
            book.Isbn);

        _store.AddCopy(copy);

        return copy;
    }

    public void IssueCopy(
        string copyId,
        string readerId,
        DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(readerId))
        {
            throw new ArgumentException(
                "Ідентифікатор читача не може бути порожнім.",
                nameof(readerId));
        }

        if (dueDate == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dueDate),
                "Дата повернення повинна бути задана.");
        }

        BookCopy copy = _store.GetCopyById(copyId)
            ?? throw new InvalidOperationException(
                $"Примірник з ID '{copyId}' не знайдено.");

        copy.Issue();

        _store.UpdateCopy(copy);

        string normalizedReaderId = readerId.Trim();

        _activeLoans[copy.Id] = (
            normalizedReaderId,
            dueDate);

        OnChanged(
            LibraryChangeKind.CopyIssued,
            copyId: copy.Id,
            readerId: normalizedReaderId,
            dueDate: dueDate);
    }

    public void ReturnCopy(string copyId)
    {
        BookCopy copy = _store.GetCopyById(copyId)
            ?? throw new InvalidOperationException(
                $"Примірник з ID '{copyId}' не знайдено.");

        copy.Return();

        _store.UpdateCopy(copy);

        string? readerId = null;
        DateTime? dueDate = null;

        if (_activeLoans.TryGetValue(copy.Id, out var loan))
        {
            readerId = loan.ReaderId;
            dueDate = loan.DueDate;

            _activeLoans.Remove(copy.Id);
        }

        OnChanged(
            LibraryChangeKind.CopyReturned,
            copyId: copy.Id,
            readerId: readerId,
            dueDate: dueDate);
    }

    public IReadOnlyList<Book> AllBooks() =>
        _store.ListBooks();

    public Book? FindBook(string id) =>
        _store.GetBookById(id);

    public IReadOnlyList<Book> SearchBooks(
        Func<Book, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _store.ListBooks()
            .Where(predicate)
            .ToList();
    }

    public IReadOnlyList<BookCopy> AllCopies() =>
        _store.ListCopies();

    public IReadOnlyList<BookCopy> FindAll(
        Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _store.ListCopies()
            .Where(predicate)
            .ToList();
    }
}

