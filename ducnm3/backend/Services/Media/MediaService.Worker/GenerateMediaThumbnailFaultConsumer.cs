using MassTransit;
using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Contracts.Messaging;
using MediaService.Infrastructure.Persistence;

namespace MediaService.Worker;

public sealed class GenerateMediaThumbnailFaultConsumer(
    IMediaDerivationRepository repository,
    TimeProvider timeProvider) : IConsumer<Fault<GenerateMediaThumbnailV1>>
{
    public Task Consume(ConsumeContext<Fault<GenerateMediaThumbnailV1>> context) =>
        repository.MarkFailedAsync(
            context.Message.Message.JobId,
            context.Message.Message.DerivativeMediaId,
            "Thumbnail generation failed after all retry attempts.",
            timeProvider.GetUtcNow().UtcDateTime,
            context.CancellationToken);
}

public sealed class GenerateMediaThumbnailFaultConsumerDefinition :
    ConsumerDefinition<GenerateMediaThumbnailFaultConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<GenerateMediaThumbnailFaultConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
    }
}
