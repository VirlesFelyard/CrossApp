using Core;
using Core.Abstractions;
using Core.Domain;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class EfBookCatalogStore(AppDbContext db) : IAsyncBookCatalogStore
{
    private static BookCatalogEntry ToDomain(BookRow row) =>
        new(Book.Create(row.Id, row.Isbn, row.Title, row.Year, row.Author), row.Copies.Count);

    private static CopyRow CreateCopy(string bookId) => new()
    {
        InventoryNumber = Guid.NewGuid().ToString("N"),
        BookId = bookId,
        State = "Available",
        IsIssued = false
    };

    public async Task<IReadOnlyList<BookCatalogEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Books.AsNoTracking()
            .Include(row => row.Copies)
            .OrderBy(row => row.Isbn)
            .ToListAsync(cancellationToken);
        return rows.Select(ToDomain).ToArray();
    }

    public async Task<BookCatalogEntry?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var row = await db.Books.AsNoTracking()
            .Include(row => row.Copies)
            .SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
        return row is null ? null : ToDomain(row);
    }

    public async Task<bool> AddAsync(Book book, int copies, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(copies);

        if (await db.Books.AnyAsync(row => row.Id == book.Id || row.Isbn == book.Isbn,
                cancellationToken))
            return false;

        var row = new BookRow
        {
            Id = book.Id,
            Isbn = book.Isbn,
            Title = book.Title,
            Author = book.Author ?? "",
            Year = book.Year
        };

        for (var i = 0; i < copies; i++)
            row.Copies.Add(CreateCopy(book.Id));

        db.Books.Add(row);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 or 1555 })
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Book book, int copies, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(copies);
        var row = await db.Books.Include(entry => entry.Copies)
            .SingleOrDefaultAsync(entry => entry.Id == book.Id, cancellationToken);
        if (row is null)
            return false;

        if (await db.Books.AnyAsync(entry => entry.Id != book.Id && entry.Isbn == book.Isbn,
                cancellationToken))
            throw new DomainException("Книга з таким ISBN вже існує.");

        row.Isbn = book.Isbn;
        row.Title = book.Title;
        row.Author = book.Author ?? "";
        row.Year = book.Year;

        var difference = copies - row.Copies.Count;
        if (difference > 0)
        {
            for (var i = 0; i < difference; i++)
                row.Copies.Add(CreateCopy(book.Id));
        }
        else if (difference < 0)
        {
            var removable = row.Copies.Where(copy => !copy.IsIssued)
                .Take(-difference).ToArray();
            if (removable.Length != -difference)
                throw new DomainException("Неможливо видалити видані примірники книги.");
            db.Copies.RemoveRange(removable);
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 or 1555 })
        {
            throw new DomainException("Книга з таким ISBN вже існує.", exception);
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var row = await db.Books.SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        if (row is null)
            return false;

        db.Books.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
