namespace Infrastructure.Persistence;

public sealed class CopyRow
{
    public string InventoryNumber { get; set; } = "";
    public string BookId { get; set; } = "";
    public string State { get; set; } = "";
    public bool IsIssued { get; set; }
    public BookRow Book { get; set; } = null!;
}
