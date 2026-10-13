using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileBookStore : IBookStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
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

        if (File.Exists(_path))
        {
            List<LibraryItemDto> items = JsonSerializer.Deserialize<List<LibraryItemDto>>(File.ReadAllText(_path)) ?? [];

            foreach (LibraryItemDto item in items)
            {
                if (item.Kind == "book" && item.Book is not null)
                {
                    Book book = Book.FromDto(item.Book);
                    _books[book.Id] = book;
                }
                else if (item.Kind == "copy" && item.Copy is not null)
                {
                    BookCopy copy = BookCopy.FromDto(item.Copy);
                    _copies[copy.Id] = copy;
                }
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        string? directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        List<LibraryItemDto> items = _books.Values
            .Select(book => new LibraryItemDto("book", Book: book.ToDto()))
            .Concat(_copies.Values.Select(copy => new LibraryItemDto("copy", Copy: copy.ToDto())))
            .ToList();

        File.WriteAllText(_path, JsonSerializer.Serialize(items, Options));
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

    public bool RemoveBook(string id)
    {
        EnsureLoaded();
        if (!_books.Remove(id))
            return false;
        Flush();
        return true;
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
