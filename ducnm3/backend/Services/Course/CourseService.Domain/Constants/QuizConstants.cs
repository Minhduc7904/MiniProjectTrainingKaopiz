// File: backend/Services/Course/CourseService.Domain/Constants/QuizConstants.cs
// Mục đích: Khai báo trạng thái Quiz, loại câu hỏi và trạng thái bài làm dùng thống nhất ở mọi layer.

namespace CourseService.Domain.Constants;

public static class QuizStatuses
{
    public const string Draft = "DRAFT";
    public const string Published = "PUBLISHED";

    public static bool IsSupported(string value) => value is Draft or Published;
}

public static class QuizQuestionTypes
{
    public const string SingleChoice = "SINGLE_CHOICE";
    public const string MultipleChoice = "MULTIPLE_CHOICE";

    public static bool IsSupported(string value) => value is SingleChoice or MultipleChoice;
}

public static class QuizAttemptStatuses
{
    public const string InProgress = "IN_PROGRESS";
    public const string Submitted = "SUBMITTED";
}
