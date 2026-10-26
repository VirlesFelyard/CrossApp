namespace Core.Reports;

public sealed record OverdueLine(string LoanId, string CopyId, string ReaderId, DateTime DueDate);
