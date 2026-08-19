// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/CourseExportPosition.cs
// Mục đích: Lưu vị trí keyset ổn định createdAtUtc DESC, id DESC của Course export.

namespace CourseService.Application.UseCases.Courses.Export;

public sealed record CourseExportPosition(DateTime CreatedAtUtc, Guid Id);
