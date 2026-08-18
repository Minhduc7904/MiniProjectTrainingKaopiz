using BuildingBlocks.Messaging.Abstractions;
using MassTransit;

namespace BuildingBlocks.Messaging;

/// <summary>
/// Đăng ký MassTransit consumer theo convention endpoint chung.
/// Dùng trong callback <c>AddLmsMessagingWithConsumers</c> để command có queue owner và event có queue subscriber riêng.
/// </summary>
public static class ConsumerRegistrationExtensions
{
    /// <summary>Đăng ký consumer command với queue được suy ra từ <paramref name="ownerService"/> và kiểu command.</summary>
    public static void AddCommandConsumer<TConsumer, TCommand>(
        this IBusRegistrationConfigurator registration,
        string ownerService)
        where TConsumer : class, IConsumer<TCommand>
        where TCommand : class, ICommand
    {
        registration
            .AddConsumer<TConsumer>()
            .Endpoint(endpoint =>
            {
                endpoint.Name = MessageEndpointNameFormatter.ForCommand(
                    ownerService,
                    typeof(TCommand));
            });
    }

    /// <summary>Biến thể command consumer cho trường hợp cần <typeparamref name="TDefinition"/> cấu hình consumer riêng.</summary>
    public static void AddCommandConsumer<TConsumer, TCommand, TDefinition>(
        this IBusRegistrationConfigurator registration,
        string ownerService)
        where TConsumer : class, IConsumer<TCommand>
        where TCommand : class, ICommand
        where TDefinition : ConsumerDefinition<TConsumer>, new()
    {
        registration
            .AddConsumer<TConsumer, TDefinition>()
            .Endpoint(endpoint =>
            {
                endpoint.Name = MessageEndpointNameFormatter.ForCommand(
                    ownerService,
                    typeof(TCommand));
            });
    }

    /// <summary>Đăng ký event consumer với queue riêng của subscriber để fan-out không làm các service chặn nhau.</summary>
    public static void AddEventConsumer<TConsumer, TEvent>(
        this IBusRegistrationConfigurator registration,
        string subscriberService)
        where TConsumer : class, IConsumer<TEvent>
        where TEvent : class, IIntegrationEvent
    {
        registration
            .AddConsumer<TConsumer>()
            .Endpoint(endpoint =>
            {
                endpoint.Name = MessageEndpointNameFormatter.ForSubscriber(
                    subscriberService,
                    typeof(TEvent));
            });
    }
}
