namespace StudentService.Application.Repositories;

public sealed record StudentListRecord(
    Guid Id,
    string Email,
    string DisplayName,
    string Status,
    DateTime CreatedAtUtc);
