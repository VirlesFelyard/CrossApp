using Core.Domain;
using Core.Dto;

namespace Core.Reports;

public static class ReportService
{
    public static IReadOnlyList<ReaderLoanLine> TopReaders(
        IEnumerable<ReaderDto> readers,
        IEnumerable<Loan> loans,
        int n)
    {
        Dictionary<string, int> counts = loans
            .GroupBy(loan => loan.ReaderId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        return readers
            .Select(r => new ReaderLoanLine(r.Id, r.Name, counts.GetValueOrDefault(r.Id)))
            .OrderByDescending(line => line.Loans)
            .ThenBy(line => line.ReaderId, StringComparer.Ordinal)
            .Take(n)
            .ToList();
    }

    public static IReadOnlyList<OverdueLine> Overdue(
        IEnumerable<Loan> loans,
        DateTime today) =>
        loans
            .Where(loan => !loan.IsClosed && loan.DueDate.HasValue && loan.DueDate.Value < today)
            .OrderBy(loan => loan.DueDate)
            .ThenBy(loan => loan.Id, StringComparer.Ordinal)
            .Select(loan => new OverdueLine(loan.Id, loan.CopyId, loan.ReaderId, loan.DueDate!.Value))
            .ToList();

    private static readonly string[] PeriodNames = ["До 1900", "1900–1949", "1950 і пізніше"];
    public static IReadOnlyList<GroupSummary> BooksByPeriod(
        IEnumerable<Book> books,
        IEnumerable<BookCopy> copies)
    {
        Dictionary<string, int> copyCounts = CountCopies(copies);

        return books
            .GroupBy(book => PeriodIndex(book.Year))
            .OrderBy(g => g.Key)
            .Select(g => new GroupSummary(
                PeriodNames[g.Key],
                g.Count(),
                g.Sum(book => copyCounts.GetValueOrDefault(book.Isbn))))
            .ToList();
    }

    public static (int Books, int Copies, double Average, int Max) Totals(
        IEnumerable<Book> books,
        IEnumerable<BookCopy> copies)
    {
        Dictionary<string, int> copyCounts = CountCopies(copies);

        List<int> perBook = books
            .Select(book => copyCounts.GetValueOrDefault(book.Isbn))
            .ToList();

        return perBook.Count == 0
            ? (0, 0, 0d, 0)
            : (perBook.Count, perBook.Sum(), perBook.Average(), perBook.Max());
    }

    public static (int Before, int After, int Snapshot) DeferredExecutionDemo(
        IEnumerable<Loan> loans,
        DateTime today)
    {
        List<Loan> source = loans.ToList();

        var query = source.Where(
            loan => !loan.IsClosed && loan.DueDate.HasValue && loan.DueDate.Value < today);

        int before = query.Count();

        source.Add(Loan.Open("L-TMP", "C-001", "R-001", today.AddDays(-30), today.AddDays(-10)));

        int after = query.Count();

        List<Loan> snapshot = query.ToList();

        source.Clear();

        return (before, after, snapshot.Count);
    }

    private static int PeriodIndex(int year) =>
        year < 1900 ? 0 : year < 1950 ? 1 : 2;

    private static Dictionary<string, int> CountCopies(IEnumerable<BookCopy> copies) =>
        copies
            .GroupBy(copy => copy.Isbn, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
}
