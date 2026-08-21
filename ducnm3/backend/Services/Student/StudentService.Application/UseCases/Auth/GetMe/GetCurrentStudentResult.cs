namespace StudentService.Application.UseCases.Auth.GetMe;

public sealed record GetCurrentStudentResult(Guid Id, string Email, string DisplayName, string Status);
