using MassTransit;
using MediaService.Application.Contracts.Messaging;
using MediaService.Application.Features.Derivations;
using MediaService.Infrastructure.Persistence;

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
