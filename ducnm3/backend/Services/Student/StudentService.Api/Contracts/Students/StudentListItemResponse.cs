namespace StudentService.Api.Contracts.Students;

public sealed record StudentListItemResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Status,
    DateTime CreatedAtUtc);
