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

    public static CourseApplicationException LessonAiJobNotFound() =>
        new(CourseErrorCodes.LessonAiJobNotFound, "Lesson AI job was not found.", 404);

    public static CourseApplicationException LessonAiJobActive() =>
        new(CourseErrorCodes.LessonAiJobActive, "A job of this type is already waiting or running for this lesson.", 409);

    public static CourseApplicationException LessonAiDocumentNotReady() =>
        new(CourseErrorCodes.LessonAiDocumentNotReady, "Every selected document must have extracted text before AI can use it.", 409);

    public static CourseApplicationException LessonContentExists() =>
        new(CourseErrorCodes.LessonContentExists, "Lesson already has content. Set overwriteContent to true to replace it.", 409);
}
