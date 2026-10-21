// File: backend/Services/Ai/AiService.Application/Common/AiBindableServices.cs
// Mục đích: Danh sách khóa được phép gán AI model: các service (loại trừ chính AI service và Gateway) và các tác vụ AI.

using AiService.Domain.Constants;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.Common;

public static class AiBindableServices
{
    public static IReadOnlyList<string> All { get; } =
    [
        ServiceNames.Course,
        ServiceNames.Student,
        ServiceNames.Media,
        ServiceNames.Notification,
        ServiceNames.Scheduler,
        ServiceNames.Admin,
        AiTaskNames.DocumentExtraction,
        AiTaskNames.LessonSynthesis,
        AiTaskNames.QuizAuthoring,
    ];

    public static bool IsSupported(string value) => All.Contains(value, StringComparer.Ordinal);
}
