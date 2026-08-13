using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Http;
using BuildingBlocks.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Communication.UnitTests;

public class MessagingFoundationTests
{
    [Test]
    public void MessageEndpointNamesAreDeterministic()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                MessageEndpointNameFormatter.ForCommand(
                    ServiceNames.Notification,
                    typeof(DispatchNotificationBatchCommand)),
                Is.EqualTo(
                    "notification-service--dispatch-notification-batch-command"));
            Assert.That(
                MessageEndpointNameFormatter.ForSubscriber(
                    ServiceNames.Media,
                    typeof(CoursePublishedEvent)),
                Is.EqualTo("media-service--course-published-event"));
        });
    }

    [Test]
    public void MessagingOptionsRejectInvalidCentralRetryConfiguration()
    {
        var options = new MessagingOptions
        {
            RabbitMq = CreateValidOptions().RabbitMq,
            Retry = new MessagingRetryOptions
            {
                RetryCount = -1,
                InitialIntervalSeconds = 1,
                IntervalIncrementSeconds = 1,
            },
            Consumer = new MessagingConsumerOptions(),
        };

        var exception = Assert.Throws<InvalidOperationException>(options.Validate);

        Assert.That(exception!.Message, Does.Contain("RetryCount"));
    }

    [Test]
    public void AddLmsMessagingRegistersTheSingleCentralRetryConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(CreateConfigurationValues())
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks();

        services.AddLmsMessaging(configuration, ServiceNames.Course);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<MessagingOptions>();

        Assert.Multiple(() =>
        {
            Assert.That(options.Retry.RetryCount, Is.EqualTo(5));
            Assert.That(options.Retry.InitialIntervalSeconds, Is.EqualTo(2));
            Assert.That(options.Retry.IntervalIncrementSeconds, Is.EqualTo(4));
        });
    }

    private static MessagingOptions CreateValidOptions() =>
        new()
        {
            RabbitMq = new RabbitMqOptions
            {
                Host = "localhost",
                Port = 5672,
                VirtualHost = "/",
                Username = "test",
                Password = "test",
            },
            Retry = new MessagingRetryOptions(),
            Consumer = new MessagingConsumerOptions(),
        };

    private static Dictionary<string, string?> CreateConfigurationValues() =>
        new()
        {
            ["Messaging:RabbitMq:Host"] = "localhost",
            ["Messaging:RabbitMq:Port"] = "5672",
            ["Messaging:RabbitMq:VirtualHost"] = "/",
            ["Messaging:RabbitMq:Username"] = "test",
            ["Messaging:RabbitMq:Password"] = "test",
            ["Messaging:Retry:RetryCount"] = "5",
            ["Messaging:Retry:InitialIntervalSeconds"] = "2",
            ["Messaging:Retry:IntervalIncrementSeconds"] = "4",
            ["Messaging:Consumer:PrefetchCount"] = "16",
            ["Messaging:Consumer:ConcurrencyLimit"] = "4",
        };

    private sealed record DispatchNotificationBatchCommand;

    private sealed record CoursePublishedEvent;
}

public class CorrelationIdDelegatingHandlerTests
{
    [Test]
    public async Task SendAsyncForwardsCurrentHttpCorrelationId()
    {
        var contextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                TraceIdentifier = "0123456789abcdef0123456789abcdef",
            },
        };
        var recordingHandler = new RecordingHandler();
        using var handler = new CorrelationIdDelegatingHandler(contextAccessor)
        {
            InnerHandler = recordingHandler,
        };
        using var client = new HttpClient(handler);

        await client.GetAsync("http://student-service/health");

        Assert.That(
            recordingHandler.CorrelationId,
            Is.EqualTo(contextAccessor.HttpContext.TraceIdentifier));
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public string? CorrelationId { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CorrelationId = request.Headers
                .GetValues(ApiHeaderNames.CorrelationId)
                .Single();
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}

public class ServiceQueryClientTests
{
    [Test]
    public async Task GetRetriesTransientFailureButPostDoesNot()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ServiceEndpoints:student-service"] = "http://student-service/",
                    ["Communication:HttpQuery:TimeoutSeconds"] = "5",
                    ["Communication:HttpQuery:RetryCount"] = "2",
                    ["Communication:HttpQuery:RetryDelayMilliseconds"] = "1",
                })
            .Build();
        var services = new ServiceCollection();
        var getHandler = new TransientResponseHandler(failuresBeforeSuccess: 2);
        services.AddServiceQueryClient<ITestQueryClient, TestQueryClient>(
            configuration,
            ServiceNames.Student);
        services.ConfigureHttpClientDefaults(client =>
            client.ConfigurePrimaryHttpMessageHandler(() => getHandler));

        using var provider = services.BuildServiceProvider();
        var queryClient = provider.GetRequiredService<ITestQueryClient>();

        var getResponse = await queryClient.GetAsync();

        Assert.Multiple(() =>
        {
            Assert.That(getResponse.IsSuccessStatusCode, Is.True);
            Assert.That(getHandler.Attempts, Is.EqualTo(3));
        });

        var postHandler = new TransientResponseHandler(failuresBeforeSuccess: 2);
        var postServices = new ServiceCollection();
        postServices.AddServiceQueryClient<ITestQueryClient, TestQueryClient>(
            configuration,
            ServiceNames.Student);
        postServices.ConfigureHttpClientDefaults(client =>
            client.ConfigurePrimaryHttpMessageHandler(() => postHandler));
        using var postProvider = postServices.BuildServiceProvider();

        var postResponse = await postProvider
            .GetRequiredService<ITestQueryClient>()
            .PostAsync();

        Assert.Multiple(() =>
        {
            Assert.That(postResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.ServiceUnavailable));
            Assert.That(postHandler.Attempts, Is.EqualTo(1));
        });
    }

    private sealed class TransientResponseHandler(int failuresBeforeSuccess)
        : HttpMessageHandler
    {
        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Attempts++;
            var statusCode = Attempts <= failuresBeforeSuccess
                ? System.Net.HttpStatusCode.ServiceUnavailable
                : System.Net.HttpStatusCode.OK;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                RequestMessage = request,
            });
        }
    }
}

public interface ITestQueryClient
{
    Task<HttpResponseMessage> GetAsync();

    Task<HttpResponseMessage> PostAsync();
}

public sealed class TestQueryClient(HttpClient httpClient) : ITestQueryClient
{
    public Task<HttpResponseMessage> GetAsync() =>
        httpClient.GetAsync("api/test");

    public Task<HttpResponseMessage> PostAsync() =>
        httpClient.PostAsync("api/test", content: null);
}
