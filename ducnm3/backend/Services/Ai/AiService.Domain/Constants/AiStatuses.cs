// File: backend/Services/Ai/AiService.Domain/Constants/AiStatuses.cs
// Mục đích: Khai báo trạng thái dùng chung cho AI provider và AI model.

namespace AiService.Domain.Constants;

public static class AiStatuses
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";

    public static bool IsSupported(string value) =>
        value is Active or Inactive;
}
