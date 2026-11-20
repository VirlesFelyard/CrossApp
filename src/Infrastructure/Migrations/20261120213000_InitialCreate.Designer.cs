using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261120213000_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("Infrastructure.Persistence.BookRow", entity =>
        {
            entity.Property<string>("Id").HasMaxLength(32).HasColumnType("TEXT");
            entity.Property<string>("Author").IsRequired().HasMaxLength(200).HasColumnType("TEXT");
            entity.Property<string>("Isbn").IsRequired().HasMaxLength(32).HasColumnType("TEXT");
            entity.Property<string>("Title").IsRequired().HasMaxLength(200).HasColumnType("TEXT");
            entity.Property<int>("Year").HasColumnType("INTEGER");
            entity.HasKey("Id");
            entity.HasIndex("Isbn").IsUnique();
            entity.ToTable("Books");
        });

        modelBuilder.Entity("Infrastructure.Persistence.CopyRow", entity =>
        {
            entity.Property<string>("InventoryNumber").HasMaxLength(32).HasColumnType("TEXT");
            entity.Property<string>("BookId").IsRequired().HasMaxLength(32).HasColumnType("TEXT");
            entity.Property<bool>("IsIssued").HasColumnType("INTEGER");
            entity.Property<string>("State").IsRequired().HasMaxLength(20).HasColumnType("TEXT");
            entity.HasKey("InventoryNumber");
            entity.HasIndex("BookId");
            entity.ToTable("Copies");
        });

        modelBuilder.Entity("Infrastructure.Persistence.CopyRow", entity =>
        {
            entity.HasOne("Infrastructure.Persistence.BookRow", "Book")
                .WithMany("Copies")
                .HasForeignKey("BookId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            entity.Navigation("Book");
        });

        modelBuilder.Entity("Infrastructure.Persistence.BookRow", entity =>
        {
            entity.Navigation("Copies");
        });
    }
}
