namespace Core.Dto;

public record ReaderDto(
    string Id,
    string Name,
    string? Email = null);