using Core.Reports;
using Xunit;

namespace Core.Tests;

public sealed class ReportServiceTests
{
    [Theory]
    [InlineData(2026, 10, 19, 0)]
    [InlineData(2026, 10, 30, 1)]
    public void Overdue_CountsOnlyActivePastDueLoans(
        int year,
        int month,
        int day,
        int expected)
    {
        DateTime today = new(year, month, day);

        int actual = ReportService.Overdue(
            TestData.OverdueLoans(),
            today).Count;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TopReaders_IsDeterministic()
    {
        var result = ReportService.TopReaders(
            TestData.Readers(),
            TestData.ReaderLoans(),
            2);

        Assert.Equal(
            new[] { "R-001", "R-002" },
            result.Select(line => line.ReaderId));
    }
}
