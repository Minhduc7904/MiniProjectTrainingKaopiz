using BuildingBlocks.Messaging.Abstractions;
using MassTransit;

namespace BuildingBlocks.Messaging;

public static class ConsumerRegistrationExtensions
{
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
