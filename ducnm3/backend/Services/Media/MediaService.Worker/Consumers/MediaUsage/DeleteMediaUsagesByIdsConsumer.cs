using MassTransit;
using MediaService.Application.UseCases.MediaUsages.DeleteByIds;
using MediaService.Contracts.Messaging;
using MediaService.Infrastructure.Persistence.Context;

namespace MediaService.Worker;

public sealed class DeleteMediaUsagesByIdsConsumer(DeleteMediaUsagesByIdsHandler handler)
    : IConsumer<DeleteMediaUsagesByIdsV1>
{
    public Task Consume(ConsumeContext<DeleteMediaUsagesByIdsV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class DeleteMediaUsagesByIdsConsumerDefinition
    : ConsumerDefinition<DeleteMediaUsagesByIdsConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<DeleteMediaUsagesByIdsConsumer> consumerConfigurator,
        IRegistrationContext context) =>
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
}
