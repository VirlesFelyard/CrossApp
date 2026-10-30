using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class ImportTests
{
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var service = new LendingService(new InMemoryBookStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                new JsonCatalogImporter(),
                "any.json",
                cts.Token));
    }
}
