namespace Core.Events;

public sealed class LibraryChangedEventArgs(
    LibraryChangeKind kind,
    string? bookId = null,
    string? copyId = null,
    string? readerId = null,
    DateTime? dueDate = null) : EventArgs
{
    public LibraryChangeKind Kind { get; } = kind;

    public string? BookId { get; } = bookId;

    public string? CopyId { get; } = copyId;

    public string? ReaderId { get; } = readerId;

    public DateTime? DueDate { get; } = dueDate;

    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}

