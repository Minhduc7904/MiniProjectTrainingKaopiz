// File: backend/Services/Ai/AiService.Application/Common/AiBindableServices.cs
// Mục đích: Danh sách service được phép gán AI model (loại trừ chính AI service và Gateway).

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
    ];

    public static bool IsSupported(string value) => All.Contains(value, StringComparer.Ordinal);
}
