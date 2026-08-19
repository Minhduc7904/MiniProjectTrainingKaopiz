// File: backend/Services/Media/MediaService.Worker/Consumers/MediaUsage/RegisterNotificationMediaUsageFaultConsumer.cs
// Mục đích: Ghi FAILED progress an toàn sau khi command Media Usage của Notification Batch đã hết transport retry.

using MassTransit;
using MediaService.Application.UseCases.MediaUsageJobs.Process;
using MediaService.Contracts.Messaging;
using MediaService.Infrastructure.Persistence.Context;

namespace MediaService.Worker;

public sealed class RegisterNotificationMediaUsageFaultConsumer(
    NotificationMediaUsageJobLifecycleHandler handler)
    : IConsumer<Fault<RegisterNotificationMediaUsageBatchV1>>
{
    public Task Consume(ConsumeContext<Fault<RegisterNotificationMediaUsageBatchV1>> context) =>
        handler.RecordFailureAsync(
            context.Message.Message,
            "Media Usage processing failed after all retry attempts.",
            context.CancellationToken);
}

public sealed class RegisterNotificationMediaUsageFaultConsumerDefinition :
    ConsumerDefinition<RegisterNotificationMediaUsageFaultConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<RegisterNotificationMediaUsageFaultConsumer> consumerConfigurator,
        IRegistrationContext context) =>
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
}
