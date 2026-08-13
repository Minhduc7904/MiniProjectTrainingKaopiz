namespace BuildingBlocks.Messaging.Abstractions;

public interface ICommand;

public interface IIntegrationEvent;

public interface ICommandSender
{
    Task SendAsync<TCommand>(
        string destinationService,
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand;
}

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;
}

public interface ICorrelationContextAccessor
{
    string? CorrelationId { get; }
}
