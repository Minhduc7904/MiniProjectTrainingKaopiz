using StudentService.Api.Contracts.Students;
using StudentService.Application.Repositories;
using StudentService.Application.UseCases.Students.GetById;

namespace StudentService.Api.Mappers;

public static class StudentResponseMapper
{
    public static StudentResponse ToResponse(GetStudentByIdResult source) =>
        new(source.Id, source.Email, source.DisplayName, source.Status);

    public static StudentListItemResponse ToResponse(StudentListRecord source) =>
        new(source.Id, source.Email, source.DisplayName, source.Status, source.CreatedAtUtc);
}
