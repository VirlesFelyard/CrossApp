namespace Core.Reports;

public static class EnumerableExtensions
{
    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, int> value,
        Func<T, string> id,
        int n) =>
        source
            .OrderByDescending(value)
            .ThenBy(id, StringComparer.Ordinal)
            .Take(n);
}
