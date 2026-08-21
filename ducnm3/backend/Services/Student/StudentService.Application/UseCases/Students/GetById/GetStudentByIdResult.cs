namespace StudentService.Application.UseCases.Students.GetById;

public sealed record GetStudentByIdResult(
    Guid Id,
    string Email,
    string DisplayName,
    string Status);
