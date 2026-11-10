using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileBookStore : IBookStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly Dictionary<string, Book> _books = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BookCopy> _copies = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path;
    private bool _loaded;

    public FileBookStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }

    private void EnsureLoaded()
    {
        if (_loaded)
            return;

        if (!File.Exists(_path))
        {
            _loaded = true;
            return;
        }

        List<LibraryItemDto?> items;
        try
        {
            string json = File.ReadAllText(_path, Encoding.UTF8);
            items = JsonSerializer.Deserialize<List<LibraryItemDto?>>(json, Options)
                ?? throw new InvalidDataException(
                    $"Некоректний каталог '{_path}': очікується JSON-масив.");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                $"Некоректний JSON у файлі каталогу '{_path}': {ex.Message}", ex);
        }

        var books = new Dictionary<string, Book>(StringComparer.OrdinalIgnoreCase);
        var copies = new Dictionary<string, BookCopy>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (LibraryItemDto? item in items)
            {
                if (item is null)
                    throw new InvalidDataException("У JSON-каталозі є null-запис.");

                if (string.Equals(item.Kind, "book", StringComparison.OrdinalIgnoreCase) &&
                    item.Book is not null && item.Copy is null)
                {
                    Book book = Book.FromDto(item.Book);
                    if (!books.TryAdd(book.Id, book))
                        throw new InvalidDataException($"Повторний ID книги: {book.Id}.");
                }
                else if (string.Equals(item.Kind, "copy", StringComparison.OrdinalIgnoreCase) &&
                         item.Copy is not null && item.Book is null)
                {
                    BookCopy copy = BookCopy.FromDto(item.Copy);
                    if (!copies.TryAdd(copy.Id, copy))
                        throw new InvalidDataException($"Повторний ID примірника: {copy.Id}.");
                }
                else
                {
                    throw new InvalidDataException(
                        "Невідомий або некоректний запис: очікується kind='book' " +
                        "з book або kind='copy' з copy.");
                }
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidDataException(
                $"Некоректні дані у JSON-каталозі '{_path}': {ex.Message}", ex);
        }

        _books.Clear();
        foreach (var pair in books)
            _books.Add(pair.Key, pair.Value);

        _copies.Clear();
        foreach (var pair in copies)
            _copies.Add(pair.Key, pair.Value);

        _loaded = true;
    }

    private void Flush() => WriteSnapshot(_books.Values, _copies.Values);

    private void WriteSnapshot(IEnumerable<Book> books, IEnumerable<BookCopy> copies)
    {
        List<LibraryItemDto> items = books
            .Select(book => new LibraryItemDto("book", Book: book.ToDto()))
            .Concat(copies.Select(copy => new LibraryItemDto("copy", Copy: copy.ToDto())))
            .ToList();

        string directory = Path.GetDirectoryName(_path)
            ?? throw new InvalidOperationException("Невірний шлях до JSON-каталогу.");
        Directory.CreateDirectory(directory);

        string tmpPath = _path + ".tmp";
        try
        {
            File.WriteAllText(
                tmpPath,
                JsonSerializer.Serialize(items, Options),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.Move(tmpPath, _path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tmpPath))
                File.Delete(tmpPath);
        }
    }

    public void ReplaceAll(IEnumerable<Book> books, IEnumerable<BookCopy> copies)
    {
        ArgumentNullException.ThrowIfNull(books);
        ArgumentNullException.ThrowIfNull(copies);

        var newBooks = new Dictionary<string, Book>(StringComparer.OrdinalIgnoreCase);
        var newCopies = new Dictionary<string, BookCopy>(StringComparer.OrdinalIgnoreCase);

        foreach (Book book in books)
        {
            ArgumentNullException.ThrowIfNull(book);
            if (!newBooks.TryAdd(book.Id, book))
                throw new InvalidDataException($"Повторний ID книги: {book.Id}.");
        }

        foreach (BookCopy copy in copies)
        {
            ArgumentNullException.ThrowIfNull(copy);
            if (!newCopies.TryAdd(copy.Id, copy))
                throw new InvalidDataException($"Повторний ID примірника: {copy.Id}.");
        }

        WriteSnapshot(newBooks.Values, newCopies.Values);

        _books.Clear();
        foreach (var pair in newBooks)
            _books.Add(pair.Key, pair.Value);

        _copies.Clear();
        foreach (var pair in newCopies)
            _copies.Add(pair.Key, pair.Value);

        _loaded = true;
    }

    public IReadOnlyList<Book> ListBooks()
    {
        EnsureLoaded();
        return _books.Values.ToList();
    }

    public Book? GetBookById(string id)
    {
        EnsureLoaded();
        return _books.GetValueOrDefault(id);
    }

    public void AddBook(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        EnsureLoaded();
        if (_books.ContainsKey(book.Id))
            throw new InvalidOperationException($"Книга з ID '{book.Id}' уже існує.");
        _books.Add(book.Id, book);
        Flush();
    }

    public void UpdateBook(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        EnsureLoaded();
        _books[book.Id] = book;
        Flush();
    }

    public IReadOnlyList<BookCopy> ListCopies()
    {
        EnsureLoaded();
        return _copies.Values.ToList();
    }

    public BookCopy? GetCopyById(string id)
    {
        EnsureLoaded();
        return _copies.GetValueOrDefault(id);
    }

    public void AddCopy(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        EnsureLoaded();
        if (_copies.ContainsKey(copy.Id))
            throw new InvalidOperationException($"Примірник з ID '{copy.Id}' уже існує.");
        _copies.Add(copy.Id, copy);
        Flush();
    }

    public void UpdateCopy(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        EnsureLoaded();
        _copies[copy.Id] = copy;
        Flush();
    }
}
