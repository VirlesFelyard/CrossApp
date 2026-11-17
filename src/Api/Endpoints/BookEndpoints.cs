using Api.Contracts;
using Api.Validation;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBooks(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("", async Task<Results<Ok<BookDto[]>, Ok<PagedBooksResponse>, ValidationProblem>> (
            string? author, int? page, int? pageSize, string? search,
            IAsyncBookCatalogStore store, CancellationToken cancellationToken) =>
        {
            if (page is <= 0)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["page"] = ["Номер сторінки повинен бути додатним."]
                });

            if (pageSize is <= 0)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["pageSize"] = ["Розмір сторінки повинен бути додатним."]
                });

            var books = await store.ListAsync(cancellationToken);
            var filtered = books
                .Where(entry => string.IsNullOrWhiteSpace(author) ||
                    (entry.Book.Author?.Contains(author, StringComparison.OrdinalIgnoreCase) ?? false))
                .Where(entry => string.IsNullOrWhiteSpace(search) ||
                    entry.Book.Isbn.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    entry.Book.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (entry.Book.Author?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

            if (page is null && pageSize is null && search is null)
                return TypedResults.Ok(filtered.Select(BookDto.From).ToArray());

            int currentPage = page ?? 1;
            int currentPageSize = pageSize ?? 20;
            var sorted = filtered.OrderBy(entry => entry.Book.Id, StringComparer.OrdinalIgnoreCase).ToArray();
            long offset = ((long)currentPage - 1) * currentPageSize;
            var items = sorted
                .Skip((int)Math.Min(offset, int.MaxValue))
                .Take(currentPageSize)
                .Select(BookDto.From)
                .ToArray();

            return TypedResults.Ok(new PagedBooksResponse(items, sorted.Length));
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
            SaveBookRequest request, IAsyncBookCatalogStore store, CancellationToken cancellationToken) =>
        {
            var book = Book.Create(Guid.NewGuid().ToString("N"),
                request.Isbn!, request.Title!, request.Year, request.Author);

            if (!await store.AddAsync(book, request.Copies, cancellationToken))
                return TypedResults.Conflict();

            return TypedResults.Created($"/api/books/{book.Id}",
                BookDto.From(new BookCatalogEntry(book, request.Copies)));
        }).AddEndpointFilter<SaveBookValidationFilter>();

        group.MapPut("/{id}", async Task<Results<Ok<BookDto>, NotFound, ValidationProblem>> (
            string id, SaveBookRequest request, IAsyncBookCatalogStore store,
            CancellationToken cancellationToken) =>
        {
            var replacement = Book.Create(id,
                request.Isbn!, request.Title!, request.Year, request.Author);

            if (!await store.UpdateAsync(replacement, request.Copies, cancellationToken))
                return TypedResults.NotFound();

            return TypedResults.Ok(
                BookDto.From(new BookCatalogEntry(replacement, request.Copies)));
        }).AddEndpointFilter<SaveBookValidationFilter>();

        group.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (
            string id, IAsyncBookCatalogStore store, CancellationToken cancellationToken) =>
        {
            if (!await store.DeleteAsync(id, cancellationToken))
                return TypedResults.NotFound();

            return TypedResults.NoContent();
        });
    }
}
