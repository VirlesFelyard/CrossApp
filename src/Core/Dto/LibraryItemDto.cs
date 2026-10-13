namespace Core.Dto;

public sealed record LibraryItemDto(
    string Kind,
    BookDto? Book = null,
    BookCopyDto? Copy = null);
