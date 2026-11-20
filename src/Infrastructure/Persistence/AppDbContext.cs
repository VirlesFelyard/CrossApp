using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BookRow> Books => Set<BookRow>();
    public DbSet<CopyRow> Copies => Set<CopyRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var book = modelBuilder.Entity<BookRow>();
        book.ToTable("Books");
        book.HasKey(row => row.Id);
        book.Property(row => row.Id).IsRequired().HasMaxLength(32);
        book.Property(row => row.Isbn).IsRequired().HasMaxLength(32);
        book.Property(row => row.Title).IsRequired().HasMaxLength(200);
        book.Property(row => row.Author).IsRequired().HasMaxLength(200);
        book.Property(row => row.Year).IsRequired();
        book.HasIndex(row => row.Isbn).IsUnique();
        book.HasMany(row => row.Copies)
            .WithOne(row => row.Book)
            .HasForeignKey(row => row.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        var copy = modelBuilder.Entity<CopyRow>();
        copy.ToTable("Copies");
        copy.HasKey(row => row.InventoryNumber);
        copy.Property(row => row.InventoryNumber).IsRequired().HasMaxLength(32);
        copy.Property(row => row.BookId).IsRequired().HasMaxLength(32);
        copy.Property(row => row.State).IsRequired().HasMaxLength(20);
        copy.Property(row => row.IsIssued).IsRequired();
    }
}
