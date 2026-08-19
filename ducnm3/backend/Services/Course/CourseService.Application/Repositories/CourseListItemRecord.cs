// File: backend/Services/Course/CourseService.Application/Repositories/CourseListItemRecord.cs
// Mục đích: Biểu diễn dữ liệu Course đọc từ persistence, không phụ thuộc HTTP contract.

namespace CourseService.Application.Repositories;

public sealed record CourseListItemRecord(
    Guid Id,
    string Name,
    string Status,
    DateTime CreatedAtUtc);
