// File: backend/Services/Media/MediaService.Worker/Consumers/Thumbnail/GenerateMediaThumbnailConsumer.cs
// Mục đích: Tiêu thụ message nền và kích hoạt nghiệp vụ GenerateMediaThumbnailConsumer.

using MassTransit;
using MediaService.Application.Contracts.Messaging;
using MediaService.Application.UseCases.MediaDerivations.GenerateThumbnail;
using MediaService.Infrastructure.Persistence.Context;

namespace MediaService.Worker;

public sealed class GenerateMediaThumbnailConsumer(
    GenerateMediaThumbnailHandler handler) : IConsumer<GenerateMediaThumbnailV1>
{
    public Task Consume(ConsumeContext<GenerateMediaThumbnailV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class GenerateMediaThumbnailConsumerDefinition :
    ConsumerDefinition<GenerateMediaThumbnailConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<GenerateMediaThumbnailConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
    }
}
