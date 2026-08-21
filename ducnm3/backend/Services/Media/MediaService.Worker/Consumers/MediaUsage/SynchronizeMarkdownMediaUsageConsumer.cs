// File: backend/Services/Media/MediaService.Worker/Consumers/MediaUsage/SynchronizeMarkdownMediaUsageConsumer.cs
// Mục đích: Nhận command đồng bộ usage media tham chiếu trong Markdown.

using MassTransit;
using MediaService.Application.UseCases.MediaUsages.SynchronizeMarkdown;
using MediaService.Contracts.Messaging;
using MediaService.Infrastructure.Persistence.Context;

namespace MediaService.Worker;

public sealed class SynchronizeMarkdownMediaUsageConsumer(
    SynchronizeMarkdownMediaUsagesHandler handler)
    : IConsumer<SynchronizeMarkdownMediaUsageV1>
{
    public Task Consume(ConsumeContext<SynchronizeMarkdownMediaUsageV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class SynchronizeMarkdownMediaUsageConsumerDefinition
    : ConsumerDefinition<SynchronizeMarkdownMediaUsageConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SynchronizeMarkdownMediaUsageConsumer> consumerConfigurator,
        IRegistrationContext context) =>
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
}
