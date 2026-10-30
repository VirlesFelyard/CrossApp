using Core.Domain;
using Core.Dto;

namespace Core.Tests;

internal static class TestData
{
    public static IReadOnlyList<ReaderDto> Readers() =>
    [
        new ReaderDto("R-002", "Другий читач"),
        new ReaderDto("R-001", "Перший читач")
    ];

    public static IReadOnlyList<Loan> ReaderLoans() =>
    [
        Loan.Open(
            "L-002",
            "C-002",
            "R-002",
            new DateTime(2026, 10, 1),
            new DateTime(2026, 10, 20)),
        Loan.Open(
            "L-001",
            "C-001",
            "R-001",
            new DateTime(2026, 10, 1),
            new DateTime(2026, 10, 20))
    ];

    public static IReadOnlyList<Loan> OverdueLoans()
    {
        Loan returned = Loan.Open(
            "L-003",
            "C-003",
            "R-003",
            new DateTime(2026, 10, 1),
            new DateTime(2026, 10, 10));

        returned.Close(new DateTime(2026, 10, 9));

        return
        [
            Loan.Open(
                "L-001",
                "C-001",
                "R-001",
                new DateTime(2026, 10, 1),
                new DateTime(2026, 10, 20)),
            Loan.Open(
                "L-002",
                "C-002",
                "R-002",
                new DateTime(2026, 10, 1),
                new DateTime(2026, 11, 5)),
            returned
        ];
    }
}
