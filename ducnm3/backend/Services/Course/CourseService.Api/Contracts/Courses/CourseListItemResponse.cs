// File: backend/Services/Course/CourseService.Api/Contracts/Courses/CourseListItemResponse.cs
// Mục đích: Khai báo dữ liệu Course được công khai qua HTTP list endpoint.

namespace CourseService.Api.Contracts.Courses;

public sealed record CourseListItemResponse(Guid Id, string Name, string Status, DateTime CreatedAtUtc);
