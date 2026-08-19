// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchContinuation.cs
// Mục đích: Cho Dispatch handler phân biệt cần enqueue chunk tiếp, đang chờ claim khác hay đã terminal để đóng Media Usage job đúng lúc.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchContinuation(
    bool ShouldDispatch,
    bool IsTerminal,
    uint SuccessCount,
    string BodyMarkdown);
