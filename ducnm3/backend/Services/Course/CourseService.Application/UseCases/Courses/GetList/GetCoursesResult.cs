// File: backend/Services/Course/CourseService.Application/UseCases/Courses/GetList/GetCoursesResult.cs
// Mục đích: Trả danh sách Course và metadata phân trang nội bộ cho use case.

using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.Courses.GetList;

public sealed record GetCoursesResult(
    IReadOnlyList<CourseListItemRecord> Items,
    long TotalItems,
    int TotalPages);
