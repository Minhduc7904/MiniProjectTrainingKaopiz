namespace StudentService.Api.Contracts.Students;

public sealed record StudentResponse(Guid Id, string Email, string DisplayName, string Status);
