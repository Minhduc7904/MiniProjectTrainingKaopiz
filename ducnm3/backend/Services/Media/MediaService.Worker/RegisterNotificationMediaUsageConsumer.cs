using MassTransit;
using MediaService.Application.Features.Usages.RegisterNotification;
using MediaService.Contracts.Messaging;
using MediaService.Infrastructure.Persistence;

namespace MediaService.Worker;

public sealed class RegisterNotificationMediaUsageConsumer(
    RegisterNotificationMediaUsagesHandler handler)
    : IConsumer<RegisterNotificationMediaUsageV1>
{
    public Task Consume(ConsumeContext<RegisterNotificationMediaUsageV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class RegisterNotificationMediaUsageBatchConsumer(
    RegisterNotificationMediaUsagesHandler handler)
    : IConsumer<RegisterNotificationMediaUsageBatchV1>
{
    public Task Consume(ConsumeContext<RegisterNotificationMediaUsageBatchV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class RegisterNotificationMediaUsageConsumerDefinition
    : ConsumerDefinition<RegisterNotificationMediaUsageConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<RegisterNotificationMediaUsageConsumer> consumerConfigurator,
        IRegistrationContext context) =>
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
}

public sealed class RegisterNotificationMediaUsageBatchConsumerDefinition
    : ConsumerDefinition<RegisterNotificationMediaUsageBatchConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<RegisterNotificationMediaUsageBatchConsumer> consumerConfigurator,
        IRegistrationContext context) =>
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
}
