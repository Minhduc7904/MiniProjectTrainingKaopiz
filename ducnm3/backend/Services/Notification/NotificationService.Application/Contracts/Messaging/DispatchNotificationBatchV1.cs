using BuildingBlocks.Messaging.Abstractions;

namespace NotificationService.Application.Contracts.Messaging;

public sealed record DispatchNotificationBatchV1(Guid BatchId) : ICommand;
