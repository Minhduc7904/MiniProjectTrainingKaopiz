using StudentService.Application.Repositories;

namespace StudentService.Application.UseCases.Students.GetList;

public sealed record GetStudentsResult(
    IReadOnlyList<StudentListRecord> Items,
    long TotalItems,
    int TotalPages);
