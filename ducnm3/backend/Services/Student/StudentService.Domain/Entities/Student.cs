namespace StudentService.Domain.Entities;

public sealed record Student(
    Guid Id,
    string Email,
    string DisplayName,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
