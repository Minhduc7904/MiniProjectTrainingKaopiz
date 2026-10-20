using System.Net;
using AiService.Infrastructure.Probe;

namespace AiService.UnitTests.Infrastructure;

public sealed class HttpAiProviderProbeTests
{
    [Test]
    public async Task SuccessCallsModelsEndpointWithBearerKey()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var probe = new HttpAiProviderProbe(new HttpClient(handler));

        var result = await probe.ProbeAsync("OPENAI", "https://api.example.test/v1/", "sk-test", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo("ONLINE"));
            Assert.That(result.HttpStatus, Is.EqualTo(200));
            Assert.That(handler.Requests.Single().RequestUri?.ToString(), Is.EqualTo("https://api.example.test/v1/models"));
            Assert.That(handler.Requests.Single().Headers.Authorization?.ToString(), Is.EqualTo("Bearer sk-test"));
            Assert.That(handler.Requests.Single().Headers.Contains("x-api-key"), Is.False);
        });
    }

    [Test]
    public async Task AnthropicAlsoSendsNativeHeaders()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var probe = new HttpAiProviderProbe(new HttpClient(handler));

        await probe.ProbeAsync("ANTHROPIC", "https://api.example.test/v1", "sk-ant", CancellationToken.None);

        var request = handler.Requests.Single();
        Assert.Multiple(() =>
        {
            Assert.That(request.Headers.GetValues("x-api-key").Single(), Is.EqualTo("sk-ant"));
            Assert.That(request.Headers.Contains("anthropic-version"), Is.True);
        });
    }

    [TestCase(HttpStatusCode.Unauthorized, "AUTH_FAILED")]
    [TestCase(HttpStatusCode.Forbidden, "AUTH_FAILED")]
    [TestCase(HttpStatusCode.NotFound, "ERROR")]
    [TestCase(HttpStatusCode.InternalServerError, "ERROR")]
    public async Task NonSuccessStatusIsClassified(HttpStatusCode status, string expected)
    {
        var probe = new HttpAiProviderProbe(new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(status)))));

        var result = await probe.ProbeAsync("CUSTOM", "https://api.example.test", "k", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(expected));
            Assert.That(result.HttpStatus, Is.EqualTo((int)status));
        });
    }

    [Test]
    public async Task NetworkFailureIsUnreachableAndDoesNotLeakTheException()
    {
        var probe = new HttpAiProviderProbe(new HttpClient(new StubHandler((_, _) => throw new HttpRequestException("secret-internal-host:5432 refused"))));

        var result = await probe.ProbeAsync("CUSTOM", "https://api.example.test", "k", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo("UNREACHABLE"));
            Assert.That(result.HttpStatus, Is.Null);
            Assert.That(result.Message, Does.Not.Contain("secret-internal-host"));
        });
    }

    [Test]
    public async Task InvalidBaseUrlIsAnErrorWithoutCallingOut()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var probe = new HttpAiProviderProbe(new HttpClient(handler));

        var result = await probe.ProbeAsync("CUSTOM", "ftp://api.example.test", "k", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo("ERROR"));
            Assert.That(handler.Requests, Is.Empty);
        });
    }

    [Test]
    public void CallerCancellationPropagates()
    {
        using var cts = new CancellationTokenSource();
        var probe = new HttpAiProviderProbe(new HttpClient(new StubHandler(async (_, token) =>
        {
            await cts.CancelAsync();
            token.ThrowIfCancellationRequested();
            return new HttpResponseMessage(HttpStatusCode.OK);
        })));

        Assert.ThrowsAsync<TaskCanceledException>(() => probe.ProbeAsync("CUSTOM", "https://api.example.test", "k", cts.Token));
    }

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return send(request, cancellationToken);
        }
    }
}
