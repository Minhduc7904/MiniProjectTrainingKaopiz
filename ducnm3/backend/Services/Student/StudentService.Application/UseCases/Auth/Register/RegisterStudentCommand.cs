namespace StudentService.Application.UseCases.Auth.Register;

public sealed record RegisterStudentCommand(string? Email, string? DisplayName);
