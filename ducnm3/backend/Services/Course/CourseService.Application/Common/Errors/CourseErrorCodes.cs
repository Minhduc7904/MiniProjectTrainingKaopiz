// File: backend/Services/Course/CourseService.Application/Common/Errors/CourseErrorCodes.cs
// Mục đích: Khai báo error code nghiệp vụ Course ổn định cho API consumer.

namespace CourseService.Application.Common.Errors;

public static class CourseErrorCodes
{
    public const string CourseNotFound = "COURSE_NOT_FOUND";
    public const string LessonNotFound = "LESSON_NOT_FOUND";
    public const string LessonOrderConflict = "LESSON_ORDER_CONFLICT";
}
