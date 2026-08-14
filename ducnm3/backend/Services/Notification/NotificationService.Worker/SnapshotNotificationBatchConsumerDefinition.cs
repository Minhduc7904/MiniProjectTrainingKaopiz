using MassTransit;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Worker;

public sealed class SnapshotNotificationBatchConsumerDefinition
    : ConsumerDefinition<SnapshotNotificationBatchConsumer>
{
    public SnapshotNotificationBatchConsumerDefinition()
    {
        ConcurrentMessageLimit = 1;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SnapshotNotificationBatchConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<NotificationDbContext>(context);
    }
}
