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
using Microsoft.Extensions.Logging;
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
        WorkerLoggingTestState.Reset();
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
        await WaitForWorkerLogsAsync(
            entry =>
                entry.MessageType == nameof(AlwaysFailNotificationCommand) &&
                entry.WorkerEvent == "Failed",
            expectedCount: 1,
            timeout: TimeSpan.FromSeconds(30));
        await WaitForWorkerLogsAsync(
            entry =>
                entry.MessageType == nameof(AlwaysFailNotificationCommand) &&
                entry.WorkerEvent == "AttemptFailed",
            expectedCount: 3,
            timeout: TimeSpan.FromSeconds(30));

        await WaitForErrorQueueAsync(
            MessageEndpointNameFormatter.ForCommand(
                ServiceNames.Notification,
                typeof(AlwaysFailNotificationCommand)) + "_error",
            TimeSpan.FromSeconds(30));

        Assert.Multiple(() =>
        {
            Assert.That(MessagingTestState.FailingAttempts, Is.EqualTo(3));
            Assert.That(
                MessagingTestState.ObservedCorrelationIds,
                Has.All.EqualTo(correlationId));

            var completedCommandLogs = WorkerLoggingTestState.Entries
                .Where(entry => entry.MessageType == nameof(CompleteNotificationCommand))
                .ToArray();
            Assert.That(
                completedCommandLogs,
                Has.Some.Matches<WorkerLogEntry>(entry =>
                    entry.WorkerEvent == "Received" &&
                    entry.CorrelationId == correlationId &&
                    entry.SourceService == ServiceNames.Course &&
                    entry.MessageId is not null &&
                    entry.Queue is not null));
            Assert.That(
                completedCommandLogs,
                Has.Some.Matches<WorkerLogEntry>(entry =>
                    entry.WorkerEvent == "Completed" &&
                    entry.CorrelationId == correlationId &&
                    entry.MessageId is not null));

            var failedCommandLogs = WorkerLoggingTestState.Entries
                .Where(entry =>
                    entry.MessageType == nameof(AlwaysFailNotificationCommand) &&
                    entry.WorkerEvent == "Failed")
                .ToArray();
            Assert.That(
                failedCommandLogs,
                Is.Not.Empty);
            Assert.That(
                failedCommandLogs,
                Has.Some.Matches<WorkerLogEntry>(entry =>
                    entry.Exception is not null &&
                    entry.CorrelationId == correlationId &&
                    entry.RetryLimit == 2));

            var retryLogs = WorkerLoggingTestState.Entries
                .Where(entry =>
                    entry.MessageType == nameof(AlwaysFailNotificationCommand) &&
                    entry.WorkerEvent == "AttemptFailed")
                .ToArray();
            Assert.That(retryLogs, Has.Length.EqualTo(3));
            Assert.That(
                retryLogs,
                Has.All.Matches<WorkerLogEntry>(entry =>
                    entry.Exception is not null &&
                    entry.CorrelationId == correlationId &&
                    entry.RetryLimit == 2));
        });

        await host.StopAsync();
    }

    private IHost CreateHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new WorkerConsumeLogCaptureProvider());
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

    private static async Task WaitForWorkerLogsAsync(
        Func<WorkerLogEntry, bool> predicate,
        int expectedCount,
        TimeSpan timeout)
    {
        using var cancellation = new CancellationTokenSource(timeout);
        while (WorkerLoggingTestState.Entries.Count(predicate) < expectedCount)
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

public sealed record WorkerLogEntry(
    string? WorkerEvent,
    string? MessageType,
    string? CorrelationId,
    string? SourceService,
    string? Queue,
    string? MessageId,
    int? RetryAttempt,
    int? RetryLimit,
    Exception? Exception);

public static class WorkerLoggingTestState
{
    private static readonly ConcurrentQueue<WorkerLogEntry> CapturedEntries = [];

    public static WorkerLogEntry[] Entries => CapturedEntries.ToArray();

    public static void Record(WorkerLogEntry entry) => CapturedEntries.Enqueue(entry);

    public static void Reset()
    {
        while (CapturedEntries.TryDequeue(out _))
        {
        }
    }
}

public sealed class WorkerConsumeLogCaptureProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) =>
        new WorkerConsumeLogCapture(categoryName);

    public void Dispose()
    {
    }

    private sealed class WorkerConsumeLogCapture(string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) =>
            categoryName == typeof(WorkerConsumeLoggingObserver).FullName;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel) ||
                state is not IEnumerable<KeyValuePair<string, object?>> properties)
            {
                return;
            }

            var values = properties.ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.Ordinal);
            WorkerLoggingTestState.Record(
                new WorkerLogEntry(
                    GetString(values, "WorkerEvent"),
                    GetString(values, "MessageType"),
                    GetString(values, "CorrelationId"),
                    GetString(values, "SourceService"),
                    GetString(values, "Queue"),
                    GetString(values, "MessageId"),
                    GetInt32(values, "RetryAttempt"),
                    GetInt32(values, "RetryLimit"),
                    exception));
        }

        private static string? GetString(
            Dictionary<string, object?> values,
            string key) =>
            values.TryGetValue(key, out var value)
                ? value?.ToString()
                : null;

        private static int? GetInt32(
            Dictionary<string, object?> values,
            string key) =>
            values.TryGetValue(key, out var value) && value is not null
                ? Convert.ToInt32(value, CultureInfo.InvariantCulture)
                : null;
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
