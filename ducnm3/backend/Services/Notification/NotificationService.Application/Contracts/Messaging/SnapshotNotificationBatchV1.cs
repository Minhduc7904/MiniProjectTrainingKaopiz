using BuildingBlocks.Messaging.Abstractions;

namespace NotificationService.Application.Contracts.Messaging;

public sealed record SnapshotNotificationBatchV1(Guid BatchId) : ICommand;
