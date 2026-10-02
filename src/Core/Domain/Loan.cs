using Core.Dto;

namespace Core.Domain;

public sealed class Loan
{
    private readonly string _id;
    private readonly string _copyId;
    private readonly string _readerId;
    private readonly DateTime _issuedOn;
    private DateTime? _returnedOn;

    public string Id => _id;
    public string CopyId => _copyId;
    public string ReaderId => _readerId;
    public DateTime IssuedOn => _issuedOn;
    public DateTime? ReturnedOn => _returnedOn;

    public bool IsClosed => _returnedOn.HasValue;

    private Loan(
        string id,
        string copyId,
        string readerId,
        DateTime issuedOn)
    {
        _id = id;
        _copyId = copyId;
        _readerId = readerId;
        _issuedOn = issuedOn;
        _returnedOn = null;
    }

    public static Loan Open(
        string id,
        string copyId,
        string readerId,
        DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор видачі не може бути порожнім",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(copyId))
        {
            throw new ArgumentException(
                "Ідентифікатор примірника не може бути порожнім",
                nameof(copyId));
        }

        if (string.IsNullOrWhiteSpace(readerId))
        {
            throw new ArgumentException(
                "Ідентифікатор читача не може бути порожнім",
                nameof(readerId));
        }

        if (issuedOn == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(issuedOn),
                issuedOn,
                "Дата видачі повинна бути задана");
        }

        return new Loan(
            id.Trim(),
            copyId.Trim(),
            readerId.Trim(),
            issuedOn);
    }

    public void Close(DateTime returnedOn)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException(
                $"Видача '{_id}' вже закрита");
        }

        if (returnedOn == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(returnedOn),
                returnedOn,
                "Дата повернення повинна бути задана");
        }

        if (returnedOn < _issuedOn)
        {
            throw new ArgumentOutOfRangeException(
                nameof(returnedOn),
                returnedOn,
                $"Дата повернення '{returnedOn:d}' не може бути раніше " +
                $"дати видачі '{_issuedOn:d}'");
        }

        _returnedOn = returnedOn;
    }

    public LoanDto ToDto() =>
        new(
            Id,
            CopyId,
            ReaderId,
            IssuedOn,
            ReturnedOn);

    public static Loan FromDto(LoanDto dto)
    {
        Loan loan = Open(
            dto.Id,
            dto.CopyId,
            dto.ReaderId,
            dto.IssuedOn);

        if (dto.ReturnedOn.HasValue)
        {
            loan.Close(dto.ReturnedOn.Value);
        }

        return loan;
    }

    public override string ToString() =>
        $"{Id} | Примірник: {CopyId} | Читач: {ReaderId} | " +
        $"Видано: {IssuedOn:d} | Повернено: " +
        (ReturnedOn.HasValue
            ? ReturnedOn.Value.ToString("d")
            : "ще не повернено");
}
