using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBooks(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("", async Task<Ok<BookDto[]>> (
            string? author, IAsyncBookCatalogStore store, CancellationToken cancellationToken) =>
        {
            var books = await store.ListAsync(cancellationToken);
            var result = books
                .Where(entry => string.IsNullOrWhiteSpace(author) ||
                    (entry.Book.Author?.Contains(author, StringComparison.OrdinalIgnoreCase) ?? false))
                .Select(BookDto.From)
                .ToArray();

            return TypedResults.Ok(result);
        });

        group.MapGet("/{id}", async Task<Results<Ok<BookDto>, NotFound>> (
            string id, IAsyncBookCatalogStore store, CancellationToken cancellationToken) =>
        {
            var entry = await store.GetByIdAsync(id, cancellationToken);
            if (entry is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(BookDto.From(entry));
        });

        group.MapPost("", async Task<Results<Created<BookDto>, ValidationProblem, Conflict>> (
            SaveBookRequest? request, IAsyncBookCatalogStore store, CancellationToken cancellationToken) =>
        {
            if (request is null)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["body"] = ["Тіло запиту обов'язкове."]
                });

            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var book = Book.Create(Guid.NewGuid().ToString("N"),
                request.Isbn!, request.Title!, request.Year, request.Author);

            if (!await store.AddAsync(book, request.Copies, cancellationToken))
                return TypedResults.Conflict();

            return TypedResults.Created($"/api/books/{book.Id}",
                BookDto.From(new BookCatalogEntry(book, request.Copies)));
        });

        group.MapPut("/{id}", async Task<Results<Ok<BookDto>, NotFound, ValidationProblem>> (
            string id, SaveBookRequest? request, IAsyncBookCatalogStore store,
            CancellationToken cancellationToken) =>
        {
            if (request is null)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["body"] = ["Тіло запиту обов'язкове."]
                });

            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var replacement = Book.Create(id,
                request.Isbn!, request.Title!, request.Year, request.Author);

            if (!await store.UpdateAsync(replacement, request.Copies, cancellationToken))
                return TypedResults.NotFound();

            return TypedResults.Ok(
                BookDto.From(new BookCatalogEntry(replacement, request.Copies)));
        });

        group.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (
            string id, IAsyncBookCatalogStore store, CancellationToken cancellationToken) =>
        {
            if (!await store.DeleteAsync(id, cancellationToken))
                return TypedResults.NotFound();

            return TypedResults.NoContent();
        });
    }
}
