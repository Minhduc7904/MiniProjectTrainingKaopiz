// File: backend/Services/Media/MediaService.Worker/Consumers/MediaUsage/RegisterNotificationMediaUsageConsumer.cs
// Mục đích: Tiêu thụ message nền và kích hoạt nghiệp vụ RegisterNotificationMediaUsageConsumer.

using MassTransit;
using MediaService.Application.UseCases.MediaUsageJobs.Process;
using MediaService.Application.UseCases.MediaUsages.RegisterNotification;
using MediaService.Application.UseCases.MediaUsages.RegisterCourseLesson;
using MediaService.Application.UseCases.MediaUsages.SynchronizeCourseContent;
using MediaService.Contracts.Messaging;
using MediaService.Infrastructure.Persistence.Context;

namespace MediaService.Worker;

public sealed class RegisterNotificationMediaUsageConsumer(
    RegisterNotificationMediaUsagesHandler handler)
    : IConsumer<RegisterNotificationMediaUsageV1>
{
    public Task Consume(ConsumeContext<RegisterNotificationMediaUsageV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class RegisterCourseLessonMediaUsageConsumer(
    RegisterCourseLessonMediaUsagesHandler handler)
    : IConsumer<RegisterCourseLessonMediaUsageV1>
{
    public Task Consume(ConsumeContext<RegisterCourseLessonMediaUsageV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class RegisterCourseLessonMediaUsageConsumerDefinition
    : ConsumerDefinition<RegisterCourseLessonMediaUsageConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<RegisterCourseLessonMediaUsageConsumer> consumerConfigurator,
        IRegistrationContext context) =>
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
}

public sealed class SynchronizeCourseContentMediaUsageConsumer(
    SynchronizeCourseContentMediaUsagesHandler handler)
    : IConsumer<SynchronizeCourseContentMediaUsageV1>
{
    public Task Consume(ConsumeContext<SynchronizeCourseContentMediaUsageV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class SynchronizeCourseContentMediaUsageConsumerDefinition
    : ConsumerDefinition<SynchronizeCourseContentMediaUsageConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SynchronizeCourseContentMediaUsageConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
    }
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

public sealed class StartNotificationMediaUsageJobConsumer(
    NotificationMediaUsageJobLifecycleHandler handler)
    : IConsumer<StartNotificationMediaUsageJobV1>
{
    public Task Consume(ConsumeContext<StartNotificationMediaUsageJobV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class CompleteNotificationMediaUsageJobConsumer(
    NotificationMediaUsageJobLifecycleHandler handler)
    : IConsumer<CompleteNotificationMediaUsageJobV1>
{
    public Task Consume(ConsumeContext<CompleteNotificationMediaUsageJobV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}

public sealed class NotificationMediaUsageJobCommandConsumerDefinition<TConsumer> :
    ConsumerDefinition<TConsumer> where TConsumer : class, IConsumer
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TConsumer> consumerConfigurator,
        IRegistrationContext context) =>
        endpointConfigurator.UseEntityFrameworkOutbox<MediaDbContext>(context);
}
