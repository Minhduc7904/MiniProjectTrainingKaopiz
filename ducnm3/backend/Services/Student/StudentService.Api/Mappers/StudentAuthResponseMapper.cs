using StudentService.Api.Contracts.Auth;
using StudentService.Api.Contracts.Auth.GetMe;
using StudentService.Application.UseCases.Auth.GetMe;
using StudentService.Application.UseCases.Auth.Login;
using StudentService.Application.UseCases.Auth.Register;

namespace StudentService.Api.Mappers;

public static class StudentAuthResponseMapper
{
    public static StudentActorResponse ToResponse(RegisterStudentResult source) =>
        new(source.Actor, source.Id);

    public static StudentActorResponse ToResponse(LoginStudentResult source) =>
        new(source.Actor, source.Id);

    public static GetCurrentStudentResponse ToResponse(GetCurrentStudentResult source) =>
        new(source.Id, source.Email, source.DisplayName, source.Status);
}
