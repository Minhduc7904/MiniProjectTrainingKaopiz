// File: backend/Services/Course/CourseService.Application/UseCases/Courses/GetList/CourseSortField.cs
// Mục đích: Giới hạn các cột sắp xếp được phép của danh sách Course.

namespace CourseService.Application.UseCases.Courses.GetList;

public enum CourseSortField
{
    CreatedAt,
    Name,
}
