using Core.Domain;
using Core.Dto;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Book> Books() =>
    [
        Book.Create("B-001", "978-617-000-001", "Кобзар", 1840, "Тарас Шевченко"),
        Book.Create("B-002", "978-617-000-002", "Захар Беркут", 1883, "Іван Франко"),
        Book.Create("B-003", "978-617-000-003", "Тигролови", 1946, "Іван Багряний"),
        Book.Create("B-004", "978-617-000-004", "Місто", 1928, "Валер'ян Підмогильний"),
        Book.Create("B-005", "978-617-000-005", "Майстер і Маргарита", 1967, "Михайло Булгаков"),
        Book.Create("B-006", "978-617-000-006", "1984", 1949, "Джордж Оруелл"),
        Book.Create("B-007", "978-617-000-007", "Сто років самотності", 1967, "Габріель Гарсія Маркес"),
        Book.Create("B-008", "978-617-000-008", "Премудрий гідальго Дон Кіхот", 1605, "Мігель де Сервантес"),
        Book.Create("B-009", "978-617-000-009", "Гаррі Поттер і філософський камінь", 1997, "Джоан Роулінг"),
        Book.Create("B-010", "978-617-000-010", "Володар перснів", 1954, "Джон Толкін"),
        Book.Create("B-011", "978-617-000-011", "Тіні забутих предків", 1911, "Михайло Коцюбинський"),
        Book.Create("B-012", "978-617-000-012", "Лісова пісня", 1911, "Леся Українка"),
        Book.Create("B-013", "978-617-000-013", "451 градус за Фаренгейтом", 1953, "Рей Бредбері"),
        Book.Create("B-014", "978-617-000-014", "Володар мух", 1954, "Вільям Голдінг"),
        Book.Create("B-015", "978-617-000-015", "Старий і море", 1952, "Ернест Гемінґвей"),
        Book.Create("B-016", "978-617-000-016", "Маленький принц", 1943, "Антуан де Сент-Екзюпері"),
        Book.Create("B-017", "978-617-000-017", "Пригоди Тома Сойєра", 1876, "Марк Твен"),
        Book.Create("B-018", "978-617-000-018", "Зачарована Десна", 1957, "Олександр Довженко")
    ];

    public static IReadOnlyList<BookCopy> Copies() =>
    [
        BookCopy.Create("C-001", "978-617-000-001"),
        BookCopy.Create("C-002", "978-617-000-002", true),
        BookCopy.Create("C-003", "978-617-000-003"),
        BookCopy.Create("C-004", "978-617-000-004"),
        BookCopy.Create("C-005", "978-617-000-005", true),
        BookCopy.Create("C-006", "978-617-000-006", true),
        BookCopy.Create("C-007", "978-617-000-007"),
        BookCopy.Create("C-008", "978-617-000-008"),
        BookCopy.Create("C-009", "978-617-000-009"),
        BookCopy.Create("C-010", "978-617-000-010"),
        BookCopy.Create("C-011", "978-617-000-001"),
        BookCopy.Create("C-012", "978-617-000-001"),
        BookCopy.Create("C-013", "978-617-000-005", true),
        BookCopy.Create("C-014", "978-617-000-005"),
        BookCopy.Create("C-015", "978-617-000-005"),
        BookCopy.Create("C-016", "978-617-000-006"),
        BookCopy.Create("C-017", "978-617-000-009"),
        BookCopy.Create("C-018", "978-617-000-009"),
        BookCopy.Create("C-019", "978-617-000-011", true),
        BookCopy.Create("C-020", "978-617-000-012"),
        BookCopy.Create("C-021", "978-617-000-013", true),
        BookCopy.Create("C-022", "978-617-000-014"),
        BookCopy.Create("C-023", "978-617-000-015"),
        BookCopy.Create("C-024", "978-617-000-016"),
        BookCopy.Create("C-025", "978-617-000-017"),
        BookCopy.Create("C-026", "978-617-000-018"),
        BookCopy.Create("C-027", "978-617-000-012")
    ];

    public static IReadOnlyList<ReaderDto> Readers() =>
    [
        new ReaderDto("R-001", "Олена Коваль", "r001@example.com"),
        new ReaderDto("R-002", "Андрій Мельник", "r002@example.com"),
        new ReaderDto("R-003", "Ірина Шевчук", "r003@example.com"),
        new ReaderDto("R-004", "Максим Бондар", "r004@example.com"),
        new ReaderDto("R-005", "Софія Ткаченко", "r005@example.com"),
        new ReaderDto("R-006", "Дмитро Кравченко", "r006@example.com"),
        new ReaderDto("R-007", "Наталія Олійник", "r007@example.com"),
        new ReaderDto("R-008", "Юрій Лисенко", "r008@example.com")
    ];

    public static IReadOnlyList<Loan> Loans() =>
    [
        Returned("L-001", "C-001", "R-001", 120, 110),
        Returned("L-002", "C-003", "R-001", 100, 90),
        Returned("L-003", "C-004", "R-001", 85, 75),
        Returned("L-004", "C-007", "R-001", 70, 60),
        Active("L-005", "C-002", "R-001", 50, -36),
        Returned("L-006", "C-008", "R-002", 110, 100),
        Returned("L-007", "C-009", "R-002", 95, 85),
        Returned("L-008", "C-010", "R-002", 80, 70),
        Active("L-009", "C-005", "R-002", 40, -26),
        Returned("L-010", "C-011", "R-003", 90, 80),
        Returned("L-011", "C-012", "R-003", 60, 50),
        Active("L-012", "C-013", "R-003", 30, -16),
        Returned("L-013", "C-014", "R-004", 75, 65),
        Returned("L-014", "C-016", "R-004", 45, 35),
        Active("L-015", "C-006", "R-004", 3, 11),
        Returned("L-016", "C-017", "R-005", 65, 55),
        Active("L-017", "C-019", "R-005", 20, -6),
        Returned("L-018", "C-020", "R-006", 55, 45),
        Active("L-019", "C-021", "R-007", 2, 12)
    ];

    private static Loan Returned(
        string id,
        string copyId,
        string readerId,
        int issuedDaysAgo,
        int returnedDaysAgo)
    {
        DateTime issuedOn = DateTime.Today.AddDays(-issuedDaysAgo);

        Loan loan = Loan.Open(id, copyId, readerId, issuedOn, issuedOn.AddDays(14));
        loan.Close(DateTime.Today.AddDays(-returnedDaysAgo));

        return loan;
    }

    private static Loan Active(
        string id,
        string copyId,
        string readerId,
        int issuedDaysAgo,
        int dueInDays) =>
        Loan.Open(
            id,
            copyId,
            readerId,
            DateTime.Today.AddDays(-issuedDaysAgo),
            DateTime.Today.AddDays(dueInDays));
}
