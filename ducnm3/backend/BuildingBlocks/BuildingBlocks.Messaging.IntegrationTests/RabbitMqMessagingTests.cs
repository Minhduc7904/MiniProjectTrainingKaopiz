using System.Collections.Concurrent;
using System.Globalization;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.RabbitMq;

namespace BuildingBlocks.Messaging.IntegrationTests;

[NonParallelizable]
public class RabbitMqMessagingTests
{
    private RabbitMqContainer rabbitMq = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        rabbitMq = new RabbitMqBuilder("rabbitmq:4.1-management")
            .WithUsername("lms_test")
            .WithPassword("lms_test_password")
            .Build();
        await rabbitMq.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await rabbitMq.DisposeAsync();
    }

    [SetUp]
    public void SetUp()
    {
        MessagingTestState.Reset();
    }

    [Test]
    public async Task CommandEventRetryAndCorrelationUseSharedTopology()
    {
        using var host = CreateHost();
        await host.StartAsync();

        using var scope = host.Services.CreateScope();
        var commandSender = scope.ServiceProvider.GetRequiredService<ICommandSender>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var messagingHealthProbe =
            scope.ServiceProvider.GetRequiredService<IMessagingHealthProbe>();
        const string correlationId = "0123456789abcdef0123456789abcdef";

        var health = await messagingHealthProbe.CheckAsync(
            TestContext.CurrentContext.CancellationToken);
        Assert.That(health.IsHealthy, Is.True);

        MessagingTestState.CorrelationId = correlationId;
        await commandSender.SendAsync(
            ServiceNames.Notification,
            new CompleteNotificationCommand(Guid.NewGuid()));
        await eventPublisher.PublishAsync(new CoursePublishedIntegrationEvent(Guid.NewGuid()));
        await commandSender.SendAsync(
            ServiceNames.Notification,
            new AlwaysFailNotificationCommand(Guid.NewGuid()));

        await MessagingTestState.CommandCompleted.Task.WaitAsync(TimeSpan.FromSeconds(30));
        await MessagingTestState.EventSubscriberA.Task.WaitAsync(TimeSpan.FromSeconds(30));
        await MessagingTestState.EventSubscriberB.Task.WaitAsync(TimeSpan.FromSeconds(30));
        await WaitForRetryAttemptsAsync(3, TimeSpan.FromSeconds(30));

        Assert.Multiple(() =>
        {
            Assert.That(MessagingTestState.FailingAttempts, Is.EqualTo(3));
            Assert.That(
                MessagingTestState.ObservedCorrelationIds,
                Has.All.EqualTo(correlationId));
        });

        await WaitForErrorQueueAsync(
            MessageEndpointNameFormatter.ForCommand(
                ServiceNames.Notification,
                typeof(AlwaysFailNotificationCommand)) + "_error",
            TimeSpan.FromSeconds(30));

        await host.StopAsync();
    }

    private IHost CreateHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Messaging:RabbitMq:Host"] = rabbitMq.Hostname,
                ["Messaging:RabbitMq:Port"] =
                    rabbitMq.GetMappedPublicPort(5672)
                        .ToString(CultureInfo.InvariantCulture),
                ["Messaging:RabbitMq:VirtualHost"] = "/",
                ["Messaging:RabbitMq:Username"] = "lms_test",
                ["Messaging:RabbitMq:Password"] = "lms_test_password",
                ["Messaging:Retry:RetryCount"] = "2",
                ["Messaging:Retry:InitialIntervalSeconds"] = "0.05",
                ["Messaging:Retry:IntervalIncrementSeconds"] = "0.05",
                ["Messaging:Consumer:PrefetchCount"] = "8",
                ["Messaging:Consumer:ConcurrencyLimit"] = "4",
            });
        builder.Services.AddHealthChecks();
        builder.Services.AddSingleton<ICorrelationContextAccessor>(
            new TestCorrelationContextAccessor());
        builder.Services.AddLmsMessagingWithConsumers(
            builder.Configuration,
            ServiceNames.Course,
            registration =>
            {
                registration
                    .AddCommandConsumer<
                        CompleteNotificationCommandConsumer,
                        CompleteNotificationCommand>(ServiceNames.Notification);
                registration
                    .AddCommandConsumer<
                        AlwaysFailNotificationCommandConsumer,
                        AlwaysFailNotificationCommand>(ServiceNames.Notification);
                registration
                    .AddEventConsumer<
                        CoursePublishedSubscriberA,
                        CoursePublishedIntegrationEvent>("subscriber-a");
                registration
                    .AddEventConsumer<
                        CoursePublishedSubscriberB,
                        CoursePublishedIntegrationEvent>("subscriber-b");
            });

        return builder.Build();
    }

    private static async Task WaitForRetryAttemptsAsync(
        int expectedAttempts,
        TimeSpan timeout)
    {
        using var cancellation = new CancellationTokenSource(timeout);
        while (MessagingTestState.FailingAttempts < expectedAttempts)
        {
            await Task.Delay(50, cancellation.Token);
        }
    }

    private async Task WaitForErrorQueueAsync(string queueName, TimeSpan timeout)
    {
        using var cancellation = new CancellationTokenSource(timeout);
        while (true)
        {
            var result = await rabbitMq.ExecAsync(
                ["rabbitmqctl", "list_queues", "name", "messages", "--formatter", "json"],
                cancellation.Token);
            if (result.ExitCode == 0 &&
                result.Stdout.Contains(queueName, StringComparison.Ordinal) &&
                result.Stdout.Contains("\"messages\":1", StringComparison.Ordinal))
            {
                return;
            }

            await Task.Delay(100, cancellation.Token);
        }
    }
}

public sealed record CompleteNotificationCommand(Guid BatchId) : ICommand;

public sealed record AlwaysFailNotificationCommand(Guid BatchId) : ICommand;

public sealed record CoursePublishedIntegrationEvent(Guid CourseId) : IIntegrationEvent;

public sealed class CompleteNotificationCommandConsumer :
    IConsumer<CompleteNotificationCommand>
{
    public Task Consume(ConsumeContext<CompleteNotificationCommand> context)
    {
        MessagingTestState.RecordCorrelation(context);
        MessagingTestState.CommandCompleted.TrySetResult();
        return Task.CompletedTask;
    }
}

public sealed class AlwaysFailNotificationCommandConsumer :
    IConsumer<AlwaysFailNotificationCommand>
{
    public Task Consume(ConsumeContext<AlwaysFailNotificationCommand> context)
    {
        MessagingTestState.RecordCorrelation(context);
        MessagingTestState.IncrementFailingAttempts();
        throw new InvalidOperationException("Expected integration-test consumer failure.");
    }
}

public sealed class CoursePublishedSubscriberA :
    IConsumer<CoursePublishedIntegrationEvent>
{
    public Task Consume(ConsumeContext<CoursePublishedIntegrationEvent> context)
    {
        MessagingTestState.RecordCorrelation(context);
        MessagingTestState.EventSubscriberA.TrySetResult();
        return Task.CompletedTask;
    }
}

public sealed class CoursePublishedSubscriberB :
    IConsumer<CoursePublishedIntegrationEvent>
{
    public Task Consume(ConsumeContext<CoursePublishedIntegrationEvent> context)
    {
        MessagingTestState.RecordCorrelation(context);
        MessagingTestState.EventSubscriberB.TrySetResult();
        return Task.CompletedTask;
    }
}

public sealed class TestCorrelationContextAccessor : ICorrelationContextAccessor
{
    public string? CorrelationId => MessagingTestState.CorrelationId;
}

public static class MessagingTestState
{
    private static int failingAttempts;

    public static TaskCompletionSource CommandCompleted { get; private set; } = CreateSource();

    public static TaskCompletionSource EventSubscriberA { get; private set; } = CreateSource();

    public static TaskCompletionSource EventSubscriberB { get; private set; } = CreateSource();

    public static ConcurrentBag<string> ObservedCorrelationIds { get; } = [];

    public static string? CorrelationId { get; set; }

    public static int FailingAttempts => Volatile.Read(ref failingAttempts);

    public static void IncrementFailingAttempts() =>
        Interlocked.Increment(ref failingAttempts);

    public static void RecordCorrelation<T>(ConsumeContext<T> context)
        where T : class
    {
        if (context.Headers.TryGetHeader(ApiHeaderNames.CorrelationId, out var value) &&
            value is not null)
        {
            ObservedCorrelationIds.Add(value.ToString()!);
        }
    }

    public static void Reset()
    {
        CommandCompleted = CreateSource();
        EventSubscriberA = CreateSource();
        EventSubscriberB = CreateSource();
        ObservedCorrelationIds.Clear();
        CorrelationId = null;
        failingAttempts = 0;
    }

    private static TaskCompletionSource CreateSource() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}
