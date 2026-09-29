namespace Core.Dto;

public sealed record ImportResult<T>(
    IReadOnlyList<T> Items,
    IReadOnlyList<string> Errors,
    int Total,
    int Accepted,
    int Skipped)
{
    public double ErrorPercentage =>
        Total == 0 ? 0 : (double)Skipped / Total * 100;

    public string Statistics =>
        $"Усього: {Total} | Прийнято: {Accepted} | Пропущено: {Skipped} | % помилок: {ErrorPercentage:F1}%";
}