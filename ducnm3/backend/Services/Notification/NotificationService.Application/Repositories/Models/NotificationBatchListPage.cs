// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchListPage.cs
// Mục đích: Mang một trang Notification Batch theo offset cùng tổng số bản ghi để API tạo metadata phân trang.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchListPage(
    IReadOnlyList<NotificationBatchSummary> Items,
    int Page,
    int PageSize,
    long TotalItems);
