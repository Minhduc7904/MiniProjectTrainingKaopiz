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

    public static CourseApplicationException QuizNotFound() =>
        new(CourseErrorCodes.QuizNotFound, "Quiz was not found.", 404);

    public static CourseApplicationException QuizHasAttempts() =>
        new(CourseErrorCodes.QuizHasAttempts, "Quiz already has attempts; its questions can no longer be changed.", 409);

    public static CourseApplicationException QuizAttemptNotFound() =>
        new(CourseErrorCodes.QuizAttemptNotFound, "Quiz attempt was not found.", 404);

    public static CourseApplicationException QuizAttemptAlreadySubmitted() =>
        new(CourseErrorCodes.QuizAttemptAlreadySubmitted, "Quiz attempt was already submitted.", 409);

    public static CourseApplicationException StudentNotEnrolled() =>
        new(CourseErrorCodes.StudentNotEnrolled, "Student is not enrolled in this course.", 403);
}
