namespace StudentService.Application.Features.Students.GetList;

public sealed record GetStudentsResult(
    IReadOnlyList<StudentListItem> Items,
    long TotalItems,
    int TotalPages);
