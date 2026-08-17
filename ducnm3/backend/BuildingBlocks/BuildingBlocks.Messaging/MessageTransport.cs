using System.Diagnostics;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Messaging;

/// <summary>Định danh service nguồn được chèn vào metadata của mọi message gửi đi.</summary>
public sealed record MessagingIdentity(string ServiceName);

/// <summary>Lấy correlation ID ưu tiên từ HTTP request, rồi đến <see cref="Activity"/> cho background flow.</summary>
public sealed class DefaultCorrelationContextAccessor(IHttpContextAccessor httpContextAccessor)
    : ICorrelationContextAccessor
{
    public string? CorrelationId =>
        httpContextAccessor.HttpContext?.TraceIdentifier ??
        Activity.Current?.TraceId.ToString();
}

/// <summary>MassTransit adapter của <see cref="ICommandSender"/>; gửi command đúng một queue owner.</summary>
public sealed class MassTransitCommandSender(
    ISendEndpointProvider sendEndpointProvider,
    ICorrelationContextAccessor correlationContextAccessor,
    MessagingIdentity identity) : ICommandSender
{
    /// <summary>Gửi command đến queue format từ service đích và kiểu message. Ném lỗi khi payload null hoặc transport không gửi được.</summary>
    public async Task SendAsync<TCommand>(
        string destinationService,
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
    {
        ArgumentNullException.ThrowIfNull(command);
        // Command dùng Send (không Publish) để chỉ owner service xử lý message.
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

    /// <summary>Gắn service nguồn và correlation metadata vào Send/Publish context; correlation GUID còn được map sang MassTransit CorrelationId.</summary>
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

/// <summary>MassTransit adapter của <see cref="IEventPublisher"/>; publish event để subscriber nhận theo topology.</summary>
public sealed class MassTransitEventPublisher(
    IPublishEndpoint publishEndpoint,
    ICorrelationContextAccessor correlationContextAccessor,
    MessagingIdentity identity) : IEventPublisher
{
    /// <summary>Publish integration event cùng metadata quan sát; task hoàn thành khi publish endpoint nhận event.</summary>
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
