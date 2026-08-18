// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationSummary.cs
// Mục đích: Biểu diễn projection Notification mà repository trả cho use case mà không lộ EF entity.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationSummary(Guid Id, Guid RecipientStudentId, string Title,
    string BodyMarkdown, string SourceType, string Status, Guid CreatedBy,
    DateTime CreatedAtUtc, DateTime? ReadAtUtc);
