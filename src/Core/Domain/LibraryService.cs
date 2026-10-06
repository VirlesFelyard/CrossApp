namespace Core.Domain;

public sealed class LibraryService
{
    private const int MaxOpenLoans = 5;

    public Loan IssueBook(BookCopy copy, string loanId, string readerId, DateTime issuedOn, IReadOnlyCollection<Loan> loans)
    {
        int openLoans = loans.Count(loan => loan.ReaderId == readerId && !loan.IsClosed);

        if (openLoans >= MaxOpenLoans)
        {
            throw new InvalidOperationException(
                $"Читач '{readerId}' вже має {MaxOpenLoans} відкритих видач");
        }

        copy.Issue();

        return Loan.Open(
            loanId,
            copy.Id,
            readerId,
            issuedOn);
    }
}