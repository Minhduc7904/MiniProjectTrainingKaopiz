// File: backend/Services/Ai/AiService.Domain/Constants/AiInvocationStatuses.cs
// Mục đích: Khai báo kết quả của một lần gọi model, lưu trong nhật ký ai_invocations.

namespace AiService.Domain.Constants;

public static class AiInvocationStatuses
{
    public const string Success = "SUCCESS";
    public const string Failed = "FAILED";
}
