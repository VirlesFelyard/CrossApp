namespace Api.Contracts;

public sealed record SaveBookRequest(
    string? Isbn,
    string? Title,
    string? Author,
    int Year,
    int Copies)
{
    public bool TryValidate(out Dictionary<string, string[]> errors)
    {
        var found = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Isbn) || Isbn.Length > 32)
            found[nameof(Isbn)] = ["ISBN обов'язковий, до 32 символів."];

        if (string.IsNullOrWhiteSpace(Title) || Title.Length > 200)
            found[nameof(Title)] = ["Назва обов'язкова, до 200 символів."];

        if (string.IsNullOrWhiteSpace(Author) || Author.Length > 200)
            found[nameof(Author)] = ["Автор обов'язковий, до 200 символів."];

        if (Year < 1450 || Year > DateTime.Now.Year)
            found[nameof(Year)] = ["Рік видання має бути від 1450 до поточного року."];

        if (Copies < 0)
            found[nameof(Copies)] = ["Кількість примірників не може бути від'ємною."];

        errors = found;
        return found.Count == 0;
    }
}
