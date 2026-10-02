using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    private readonly string _id;
    private readonly string _isbn;
    private bool _isIssued;

    public string Id => _id;
    public string Isbn => _isbn;
    public bool IsIssued => _isIssued;

    private BookCopy(
        string id,
        string isbn,
        bool isIssued)
    {
        _id = id;
        _isbn = isbn;
        _isIssued = isIssued;
    }

    public static BookCopy Create(
        string id,
        string isbn,
        bool isIssued = false)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор примірника не може бути порожнім",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException(
                "ISBN не може бути порожнім",
                nameof(isbn));
        }

        return new BookCopy(
            id.Trim(),
            isbn.Trim(),
            isIssued);
    }

    public void Issue()
    {
        if (_isIssued)
        {
            throw new InvalidOperationException(
                $"Примірник '{_id}' вже виданий");
        }

        _isIssued = true;
    }

    public void Return()
    {
        if (!_isIssued)
        {
            throw new InvalidOperationException(
                $"Примірник '{_id}' не є виданим");
        }

        _isIssued = false;
    }

    public BookCopyDto ToDto() =>
        new(
            Id,
            Isbn,
            IsIssued);

    public static BookCopy FromDto(BookCopyDto dto) =>
        Create(
            dto.Id,
            dto.Isbn,
            dto.IsIssued);

    public override string ToString() =>
        $"{Id} | ISBN: {Isbn} | Виданий: {IsIssued}";
}
