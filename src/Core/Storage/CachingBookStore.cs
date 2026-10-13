using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingBookStore : IBookStore
{
    private readonly IBookStore _inner;
    private IReadOnlyList<Book>? _booksCache;
    private IReadOnlyList<BookCopy>? _copiesCache;
    private readonly Dictionary<string, Book?> _bookByIdCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BookCopy?> _copyByIdCache = new(StringComparer.OrdinalIgnoreCase);

    public int CacheHits { get; private set; }
    public int CacheMisses { get; private set; }

    public CachingBookStore(IBookStore inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyList<Book> ListBooks()
    {
        if (_booksCache is not null)
        {
            CacheHits++;
            return _booksCache;
        }

        CacheMisses++;
        _booksCache = _inner.ListBooks().ToList();
        return _booksCache;
    }

    public Book? GetBookById(string id)
    {
        if (_bookByIdCache.TryGetValue(id, out Book? book))
        {
            CacheHits++;
            return book;
        }

        CacheMisses++;
        book = _inner.GetBookById(id);
        _bookByIdCache[id] = book;
        return book;
    }

    public void AddBook(Book book)
    {
        _inner.AddBook(book);
        Invalidate();
    }

    public void UpdateBook(Book book)
    {
        _inner.UpdateBook(book);
        Invalidate();
    }

    public IReadOnlyList<BookCopy> ListCopies()
    {
        if (_copiesCache is not null)
        {
            CacheHits++;
            return _copiesCache;
        }

        CacheMisses++;
        _copiesCache = _inner.ListCopies().ToList();
        return _copiesCache;
    }

    public BookCopy? GetCopyById(string id)
    {
        if (_copyByIdCache.TryGetValue(id, out BookCopy? copy))
        {
            CacheHits++;
            return copy;
        }

        CacheMisses++;
        copy = _inner.GetCopyById(id);
        _copyByIdCache[id] = copy;
        return copy;
    }

    public void AddCopy(BookCopy copy)
    {
        _inner.AddCopy(copy);
        Invalidate();
    }

    public void UpdateCopy(BookCopy copy)
    {
        _inner.UpdateCopy(copy);
        Invalidate();
    }

    private void Invalidate()
    {
        _booksCache = null;
        _copiesCache = null;
        _bookByIdCache.Clear();
        _copyByIdCache.Clear();
    }
}
