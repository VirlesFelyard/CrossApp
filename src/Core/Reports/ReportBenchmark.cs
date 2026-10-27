using System.Diagnostics;
using Core.Domain;

namespace Core.Reports;

public static class ReportBenchmark
{
    public static BenchmarkResult Compare(int count, DateTime today)
    {
        List<Loan> loans = Enumerable.Range(0, count)
            .Select(i => CreateLoan(i, today))
            .ToList();

        double withoutToList = Measure(() => WithoutToList(loans, today));
        double withToList = Measure(() => WithToList(loans, today));

        return new BenchmarkResult(count, withoutToList, withToList);
    }

    private static double Measure(Func<IReadOnlyList<ReaderLoanLine>> report)
    {
        report();

        Stopwatch stopwatch = Stopwatch.StartNew();
        report();
        stopwatch.Stop();

        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static IReadOnlyList<ReaderLoanLine> WithoutToList(
        List<Loan> loans,
        DateTime today) =>
        loans
            .Where(loan => IsOverdue(loan, today))
            .GroupBy(loan => loan.ReaderId)
            .Select(g => new ReaderLoanLine(g.Key, g.Key, g.Count()))
            .TopBy(line => line.Loans, line => line.ReaderId, 5)
            .ToList();

    private static IReadOnlyList<ReaderLoanLine> WithToList(
        List<Loan> loans,
        DateTime today) =>
        loans
            .Where(loan => IsOverdue(loan, today))
            .ToList()
            .GroupBy(loan => loan.ReaderId)
            .Select(g => new ReaderLoanLine(g.Key, g.Key, g.Count()))
            .ToList()
            .TopBy(line => line.Loans, line => line.ReaderId, 5)
            .ToList();

    private static bool IsOverdue(Loan loan, DateTime today) =>
        !loan.IsClosed && loan.DueDate.HasValue && loan.DueDate.Value < today;

    private static Loan CreateLoan(int index, DateTime today)
    {
        DateTime issuedOn = today.AddDays(-(index % 90));

        Loan loan = Loan.Open(
            $"L-{index}",
            $"C-{index % 5000}",
            $"R-{index % 1000}",
            issuedOn,
            issuedOn.AddDays(14));

        if (index % 3 == 0)
            loan.Close(today);

        return loan;
    }
}
