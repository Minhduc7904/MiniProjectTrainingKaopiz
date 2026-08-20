// File: backend/Services/Course/CourseService.Application/Common/Errors/CourseErrors.cs
// Mục đích: Tập trung tạo lỗi Course an toàn, nhất quán với Common Errors của Media Service.

using BuildingBlocks.Contracts.Api;

namespace CourseService.Application.Common.Errors;

public static class CourseErrors
{
    public static CourseApplicationException ValidationFailed(
        IReadOnlyList<ApiErrorDetail> details) =>
        new(
            ApiErrorCodes.ValidationFailed,
            ApiErrorMessages.ValidationFailed,
            400,
            details);

    public static CourseApplicationException CourseNotFound() =>
        new(CourseErrorCodes.CourseNotFound, "Course was not found.", 404);

    public static CourseApplicationException LessonNotFound() =>
        new(CourseErrorCodes.LessonNotFound, "Lesson was not found.", 404);

    public static CourseApplicationException LessonOrderConflict() =>
        new(CourseErrorCodes.LessonOrderConflict, "Lesson displayOrder already exists.", 409);
}
