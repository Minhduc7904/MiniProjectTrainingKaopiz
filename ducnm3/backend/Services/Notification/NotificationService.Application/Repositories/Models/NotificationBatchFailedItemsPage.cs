// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchFailedItemsPage.cs
// Mục đích: Đóng gói một trang item thất bại cùng cursor và cờ còn trang tiếp theo.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchFailedItemsPage(
    IReadOnlyList<NotificationBatchFailedItem> Items, Guid? NextItemId, bool HasNextPage);
