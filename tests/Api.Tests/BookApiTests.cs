using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.Tests;

public sealed class BookApiTests
{
    [Fact]
    public async Task PostBook_ThenGetLocation_Returns201And200()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var created = await client.PostAsJsonAsync("/api/books", new
        {
            isbn = "9786171234567",
            title = "Тестова книга",
            author = "Тестовий автор",
            year = 2024,
            copies = 2
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var location = Assert.IsType<Uri>(created.Headers.Location);

        using var found = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, found.StatusCode);
    }
}
