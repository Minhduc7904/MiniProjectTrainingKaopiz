// File: backend/Services/Notification/NotificationService.Application/Contracts/Messaging/SnapshotNotificationBatchV1.cs
// Mục đích: Khai báo hợp đồng message SnapshotNotificationBatchV1 dùng để giao tiếp bất đồng bộ giữa các service.

using BuildingBlocks.Messaging.Abstractions;

namespace NotificationService.Application.Contracts.Messaging;

public sealed record SnapshotNotificationBatchV1(Guid BatchId) : ICommand;
