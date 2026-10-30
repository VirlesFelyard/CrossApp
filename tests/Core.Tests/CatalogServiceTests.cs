using Core;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class CatalogServiceTests
{
    [Fact]
    public void Return_WhenCopyIsNotIssued_ThrowsDomainException_AndKeepsState()
    {
        BookCopy copy = BookCopy.Create("C-001", "978-617-000-001");

        Assert.Throws<DomainException>(() => copy.Return());
        Assert.False(copy.IsIssued);
    }

    [Fact]
    public void AddBook_RaisesChangedExactlyOnce()
    {
        var service = new LendingService(new InMemoryBookStore());
        var events = new List<LibraryChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.AddBook(
            "978-617-000-001",
            "Кобзар",
            1840,
            "Тарас Шевченко");

        Assert.Single(events);
        Assert.Equal(LibraryChangeKind.BookAdded, events[0].Kind);
    }
}
