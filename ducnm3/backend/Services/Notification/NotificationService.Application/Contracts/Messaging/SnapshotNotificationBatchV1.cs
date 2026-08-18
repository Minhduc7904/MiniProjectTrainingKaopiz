// File: backend/Services/Notification/NotificationService.Application/Contracts/Messaging/SnapshotNotificationBatchV1.cs
// Mục đích: Mang BatchId đến Worker để chụp danh sách student recipient trước khi bắt đầu dispatch.

using BuildingBlocks.Messaging.Abstractions;

namespace NotificationService.Application.Contracts.Messaging;

public sealed record SnapshotNotificationBatchV1(Guid BatchId) : ICommand;
