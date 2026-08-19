// File: backend/Services/Course/CourseService.Api/Mappers/CourseResponseMapper.cs
// Mục đích: Chuyển dữ liệu use case Course thành HTTP contract công khai.

using CourseService.Api.Contracts.Courses;
using CourseService.Application.Repositories;

namespace CourseService.Api.Mappers;

public static class CourseResponseMapper
{
    public static IReadOnlyList<CourseListItemResponse> ToListResponse(
        IReadOnlyList<CourseListItemRecord> items) =>
        items.Select(item => new CourseListItemResponse(
            item.Id, item.Name, item.Status, item.CreatedAtUtc)).ToArray();
}
