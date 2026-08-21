// File: backend/Services/Media/MediaService.Worker/Consumers/MediaUsage/MediaBackgroundJobFaultConsumers.cs
// Mục đích: Đánh dấu failed cho Markdown sync và batch xóa usage khi command đã hết transport retry.

using MassTransit;
using MediaService.Application.UseCases.MediaUsageJobs.Process;
using MediaService.Contracts.Messaging;
using MediaService.Infrastructure.Persistence.Context;

namespace MediaService.Worker;

public sealed class SynchronizeMarkdownMediaUsageFaultConsumer(
    MediaBackgroundJobLifecycleHandler handler)
    : IConsumer<Fault<SynchronizeMarkdownMediaUsageV1>>
{
    public Task Consume(ConsumeContext<Fault<SynchronizeMarkdownMediaUsageV1>> context)
    {
        var command = context.Message.Message;
        return command.JobId == Guid.Empty
            ? Task.CompletedTask
            : handler.FailAsync(
                command.JobId,
                checked((uint)(command.Added.Count + command.Removed.Count)),
                "Markdown media usage synchronization failed after all retry attempts.",
                context.CancellationToken);
    }
}

public sealed class SynchronizeMarkdownMediaUsageFaultConsumerDefinition
    : ConsumerDefinition<SynchronizeMarkdownMediaUsageFaultConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SynchronizeMarkdownMediaUsageFaultConsumer> consumerConfigurator,
        IRegistrationContext context) =>
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
}

public sealed class DeleteMediaUsagesByIdsFaultConsumer(
    MediaBackgroundJobLifecycleHandler handler)
    : IConsumer<Fault<DeleteMediaUsagesByIdsV1>>
{
    public Task Consume(ConsumeContext<Fault<DeleteMediaUsagesByIdsV1>> context)
    {
        var command = context.Message.Message;
        return command.JobId == Guid.Empty
            ? Task.CompletedTask
            : handler.FailAsync(
                command.JobId,
                checked((uint)command.UsageIds.Distinct().Count(id => id != Guid.Empty)),
                "Media usage batch deletion failed after all retry attempts.",
                context.CancellationToken);
    }
}

public sealed class DeleteMediaUsagesByIdsFaultConsumerDefinition
    : ConsumerDefinition<DeleteMediaUsagesByIdsFaultConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<DeleteMediaUsagesByIdsFaultConsumer> consumerConfigurator,
        IRegistrationContext context) =>
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
}
