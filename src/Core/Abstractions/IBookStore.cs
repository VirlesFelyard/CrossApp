using Core.Domain;

namespace Core.Abstractions;

public interface IBookStore
{
    IReadOnlyList<Book> ListBooks();
    Book? GetBookById(string id);
    void AddBook(Book book);
    void UpdateBook(Book book);
    bool RemoveBook(string id);
    IReadOnlyList<BookCopy> ListCopies();
    BookCopy? GetCopyById(string id);
    void AddCopy(BookCopy copy);
    void UpdateCopy(BookCopy copy);
}
