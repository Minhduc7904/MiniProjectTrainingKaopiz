// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/CourseExportPosition.cs
// Mục đích: Lưu vị trí keyset ổn định createdAtUtc DESC, id DESC của Course export.

namespace CourseService.Application.UseCases.Courses.Export;

// Cursor nội bộ, không trả ra API: cặp (CreatedAtUtc, Id) tạo total order cho keyset pagination;
// ReadCount giúp áp dụng limit cho cả file khi export chạy qua nhiều chunk.
public sealed record CourseExportPosition(DateTime CreatedAtUtc, Guid Id, int ReadCount = 0);
