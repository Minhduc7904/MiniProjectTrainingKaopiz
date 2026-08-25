using System.Net;
using System.Text;
using BuildingBlocks.Contracts.Api;
using Lms.PerformanceRunner.Http;

namespace Lms.PerformanceRunner.UnitTests.Http;

public sealed class NotificationBenchmarkClientTests
{
    private const string PerformanceActorIdName = "PERFORMANCE_ACTOR_ID";
    private static readonly Guid ActorId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public async Task RunAsyncWhenActorIdConfiguredInEnvironmentSendsAdminActorHeadersOnEveryRequest()
    {
        var previousActorId = Environment.GetEnvironmentVariable(PerformanceActorIdName);
        Environment.SetEnvironmentVariable(PerformanceActorIdName, ActorId.ToString());
        try
        {
            using var client = new HttpClient(new NotificationBatchHandler(ActorId))
            {
                BaseAddress = new Uri("http://localhost:5100"),
            };
            var benchmarkClient = new NotificationBenchmarkClient(client);

            var result = await benchmarkClient.RunAsync(3_000, 1, CancellationToken.None);

            Assert.That(result.FinalStatus, Is.EqualTo("COMPLETED"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(PerformanceActorIdName, previousActorId);
        }
    }

    private sealed class NotificationBatchHandler(Guid expectedActorId) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Assert.Multiple(() =>
            {
                Assert.That(request.Headers.GetValues(ApiHeaderNames.ActorType).Single(), Is.EqualTo(ActorHeaderTypes.Admin));
                Assert.That(request.Headers.GetValues(ApiHeaderNames.ActorId).Single(), Is.EqualTo(expectedActorId.ToString()));
            });

            var body = request.Method == HttpMethod.Post
                ? "{\"data\":{\"id\":\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\"}}"
                : "{\"data\":{\"status\":\"COMPLETED\",\"totalCount\":3000,\"processedCount\":3000,\"successCount\":3000,\"failedCount\":0,\"durationMs\":1000}}";
            return Task.FromResult(new HttpResponseMessage(
                request.Method == HttpMethod.Post ? HttpStatusCode.Accepted : HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }
}
