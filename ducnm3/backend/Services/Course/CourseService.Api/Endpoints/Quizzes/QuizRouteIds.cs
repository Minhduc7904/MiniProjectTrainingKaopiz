// File: backend/Services/Course/CourseService.Api/Endpoints/Quizzes/QuizRouteIds.cs
// Mục đích: Parse ID trên route Quiz; ID sai hoặc UUID rỗng trả 400 VALIDATION_FAILED.

using CourseService.Application.Common.Errors;

namespace CourseService.Api.Endpoints.Quizzes;

internal static class QuizRouteIds
{
    public static Guid Require(string? value) =>
        Guid.TryParse(value, out var id) && id != Guid.Empty ? id : throw CourseErrors.ValidationFailed([]);
}
