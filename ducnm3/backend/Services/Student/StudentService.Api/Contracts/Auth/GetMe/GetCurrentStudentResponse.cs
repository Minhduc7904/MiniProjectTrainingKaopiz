namespace StudentService.Api.Contracts.Auth.GetMe;

public sealed record GetCurrentStudentResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Status);
