using System.Diagnostics;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Messaging;

public sealed record MessagingIdentity(string ServiceName);

public sealed class DefaultCorrelationContextAccessor(IHttpContextAccessor httpContextAccessor)
    : ICorrelationContextAccessor
{
    public string? CorrelationId =>
        httpContextAccessor.HttpContext?.TraceIdentifier ??
        Activity.Current?.TraceId.ToString();
}

public sealed class MassTransitCommandSender(
    ISendEndpointProvider sendEndpointProvider,
    ICorrelationContextAccessor correlationContextAccessor,
    MessagingIdentity identity) : ICommandSender
{
    public async Task SendAsync<TCommand>(
        string destinationService,
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
    {
        ArgumentNullException.ThrowIfNull(command);
        var endpointName = MessageEndpointNameFormatter.ForCommand(
            destinationService,
            typeof(TCommand));
        var endpoint = await sendEndpointProvider.GetSendEndpoint(
            new Uri($"queue:{endpointName}"));

        await endpoint.Send(
            command,
            context => ApplyMetadata(
                context,
                identity.ServiceName,
                correlationContextAccessor.CorrelationId),
            cancellationToken);
    }

    internal static void ApplyMetadata<T>(
        SendContext<T> context,
        string sourceService,
        string? correlationId)
        where T : class
    {
        context.Headers.Set(MessagingHeaderNames.SourceService, sourceService);

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return;
        }

        context.Headers.Set(ApiHeaderNames.CorrelationId, correlationId);
        if (Guid.TryParse(correlationId, out var parsedCorrelationId))
        {
            context.CorrelationId = parsedCorrelationId;
        }
    }
}

public sealed class MassTransitEventPublisher(
    IPublishEndpoint publishEndpoint,
    ICorrelationContextAccessor correlationContextAccessor,
    MessagingIdentity identity) : IEventPublisher
{
    public Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        return publishEndpoint.Publish(
            integrationEvent,
            context => MassTransitCommandSender.ApplyMetadata(
                context,
                identity.ServiceName,
                correlationContextAccessor.CorrelationId),
            cancellationToken);
    }
}
