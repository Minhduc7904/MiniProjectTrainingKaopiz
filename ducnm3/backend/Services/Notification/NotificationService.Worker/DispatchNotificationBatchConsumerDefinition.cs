using MassTransit;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Worker;

public sealed class DispatchNotificationBatchConsumerDefinition
    : ConsumerDefinition<DispatchNotificationBatchConsumer>
{
    public DispatchNotificationBatchConsumerDefinition()
    {
        ConcurrentMessageLimit = 1;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<DispatchNotificationBatchConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<NotificationDbContext>(context);
    }
}
