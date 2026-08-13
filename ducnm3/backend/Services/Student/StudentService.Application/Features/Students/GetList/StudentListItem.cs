namespace StudentService.Application.Features.Students.GetList;

public sealed record StudentListItem(
    Guid Id,
    string Email,
    string DisplayName,
    string Status,
    DateTime CreatedAtUtc);
