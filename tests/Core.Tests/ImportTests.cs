using System.Runtime.CompilerServices;
using Core.Abstractions;
using Core.Domain;
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

    [Fact]
    public async Task ImportAsync_WhenTimeoutExpires_CancelsByTimeout()
    {
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();
        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(100));

        var service = new LendingService(new InMemoryBookStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                new SlowCatalogImporter(),
                "unused.json",
                userCts.Token,
                timeoutCts.Token));

        Assert.False(userCts.IsCancellationRequested);
        Assert.True(timeoutCts.IsCancellationRequested);
    }

    [Fact]
    public async Task ImportAsync_StreamingImport_Reports100Percent()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(
                path,
                """
                [
                  {
                    "id": "B-T01",
                    "isbn": "978-617-999-101",
                    "title": "Тестова книга 1",
                    "year": 2020,
                    "author": "Автор 1"
                  },
                  {
                    "id": "B-T02",
                    "isbn": "978-617-999-102",
                    "title": "Тестова книга 2",
                    "year": 2021,
                    "author": "Автор 2"
                  }
                ]
                """);

            var service = new LendingService(new InMemoryBookStore());
            var progress = new RecordingProgress();

            int added = await service.ImportAsync(
                new JsonCatalogImporter(),
                path,
                progress: progress);

            Assert.Equal(2, added);
            Assert.NotEmpty(progress.Values);
            Assert.Equal(100, progress.Values[^1]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private sealed class SlowCatalogImporter : ICatalogImporter
    {
        public async IAsyncEnumerable<Book> LoadAsync(
            string path,
            [EnumeratorCancellation] CancellationToken ct = default,
            IProgress<int>? progress = null)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
            yield break;
        }
    }

    private sealed class RecordingProgress : IProgress<int>
    {
        public List<int> Values { get; } = [];

        public void Report(int value) => Values.Add(value);
    }
}
