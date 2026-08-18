// File: backend/Services/Notification/NotificationService.Application/Contracts/Messaging/DispatchNotificationBatchV1.cs
// Mục đích: Mang BatchId đến Worker để claim và gửi chunk recipient tiếp theo của Notification Batch.

using BuildingBlocks.Messaging.Abstractions;

namespace NotificationService.Application.Contracts.Messaging;

public sealed record DispatchNotificationBatchV1(Guid BatchId) : ICommand;
