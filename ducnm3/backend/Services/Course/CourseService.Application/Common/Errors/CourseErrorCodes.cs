// File: backend/Services/Course/CourseService.Application/Common/Errors/CourseErrorCodes.cs
// Mục đích: Khai báo error code nghiệp vụ Course ổn định cho API consumer.

namespace CourseService.Application.Common.Errors;

public static class CourseErrorCodes
{
    public const string CourseNotFound = "COURSE_NOT_FOUND";
    public const string LessonNotFound = "LESSON_NOT_FOUND";
    public const string LessonOrderConflict = "LESSON_ORDER_CONFLICT";
    public const string QuizNotFound = "QUIZ_NOT_FOUND";
    public const string QuizHasAttempts = "QUIZ_HAS_ATTEMPTS";
    public const string QuizAttemptNotFound = "QUIZ_ATTEMPT_NOT_FOUND";
    public const string QuizAttemptAlreadySubmitted = "QUIZ_ATTEMPT_ALREADY_SUBMITTED";
    public const string StudentNotEnrolled = "STUDENT_NOT_ENROLLED";
    public const string LessonAiJobNotFound = "LESSON_AI_JOB_NOT_FOUND";
    public const string LessonAiJobActive = "LESSON_AI_JOB_ACTIVE";
    public const string LessonAiDocumentNotReady = "LESSON_AI_DOCUMENT_NOT_READY";
    public const string LessonContentExists = "LESSON_CONTENT_EXISTS";
}
