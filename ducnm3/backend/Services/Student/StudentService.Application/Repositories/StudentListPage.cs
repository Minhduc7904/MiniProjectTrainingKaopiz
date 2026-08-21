namespace StudentService.Application.Repositories;

public sealed record StudentListPage(
    IReadOnlyList<StudentListRecord> Items,
    long TotalItems,
    int TotalPages);
