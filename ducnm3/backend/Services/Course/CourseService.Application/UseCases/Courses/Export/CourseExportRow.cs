// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/CourseExportRow.cs
// Mục đích: Biểu diễn một dòng Course độc lập với HTTP và persistence projection.

namespace CourseService.Application.UseCases.Courses.Export;

public sealed record CourseExportRow(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedAtUtc);
