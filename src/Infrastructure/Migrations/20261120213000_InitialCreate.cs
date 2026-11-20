using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Books",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                Isbn = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Author = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Year = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Books", row => row.Id);
            });

        migrationBuilder.CreateTable(
            name: "Copies",
            columns: table => new
            {
                InventoryNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                BookId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                State = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                IsIssued = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Copies", row => row.InventoryNumber);
                table.ForeignKey(
                    name: "FK_Copies_Books_BookId",
                    column: row => row.BookId,
                    principalTable: "Books",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Books_Isbn",
            table: "Books",
            column: "Isbn",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Copies_BookId",
            table: "Copies",
            column: "BookId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Copies");
        migrationBuilder.DropTable(name: "Books");
    }
}
