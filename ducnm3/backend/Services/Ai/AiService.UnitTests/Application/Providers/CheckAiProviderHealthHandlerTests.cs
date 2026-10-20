using AiService.Application.Common.Errors;
using AiService.Application.Services;
using AiService.Application.UseCases.Providers.HealthCheck;
using AiService.UnitTests.Support;

namespace AiService.UnitTests.Application.Providers;

public sealed class CheckAiProviderHealthHandlerTests
{
    private InMemoryAiStore _store = null!;
    private FakeAiProviderProbe _probe = null!;
    private CheckAiProviderHealthHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _store = new InMemoryAiStore();
        _probe = new FakeAiProviderProbe();
        _handler = new CheckAiProviderHealthHandler(_store.ProviderRepository, new FakeSecretProtector(), _probe);
    }

    [Test]
    public async Task ActiveProviderIsProbedWithTheDecryptedKey()
    {
        var provider = _store.AddProvider();

        var result = await _handler.HandleAsync(provider.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.HealthStatus, Is.EqualTo("ONLINE"));
            Assert.That(result.HttpStatus, Is.EqualTo(200));
            Assert.That(result.Provider.Id, Is.EqualTo(provider.Id));
            Assert.That(_probe.Calls, Has.Count.EqualTo(1));
            Assert.That(_probe.Calls[0].ApiKey, Is.EqualTo("sk-original-abcd"));
            Assert.That(_probe.Calls[0].BaseUrl, Is.EqualTo(provider.BaseUrl));
        });
    }

    [Test]
    public async Task InactiveProviderIsReportedDisabledWithoutCallingOut()
    {
        var provider = _store.AddProvider(status: "INACTIVE");

        var result = await _handler.HandleAsync(provider.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.HealthStatus, Is.EqualTo("DISABLED"));
            Assert.That(result.LatencyMs, Is.Null);
            Assert.That(_probe.Calls, Is.Empty);
        });
    }

    [Test]
    public async Task ProbeFailureIsPassedThrough()
    {
        var provider = _store.AddProvider();
        _probe.Setup(provider.BaseUrl, new AiProviderProbeResult("AUTH_FAILED", 401, 80, "Provider rejected the API key."));

        var result = await _handler.HandleAsync(provider.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.HealthStatus, Is.EqualTo("AUTH_FAILED"));
            Assert.That(result.HttpStatus, Is.EqualTo(401));
        });
    }

    [Test]
    public async Task UndecryptableKeyIsReportedAsErrorWithoutCallingOut()
    {
        var provider = _store.AddProvider();
        _store.Providers[provider.Id] = _store.Providers[provider.Id] with { ApiKeyEncrypted = "!!not-base64!!" };
        var handler = new CheckAiProviderHealthHandler(_store.ProviderRepository, new ThrowingSecretProtector(), _probe);

        var result = await handler.HandleAsync(provider.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.HealthStatus, Is.EqualTo("ERROR"));
            Assert.That(_probe.Calls, Is.Empty);
        });
    }

    [Test]
    public void UnknownProviderThrowsNotFoundAndEmptyIdThrowsValidation()
    {
        var notFound = Assert.ThrowsAsync<AiApplicationException>(() => _handler.HandleAsync(Guid.NewGuid(), CancellationToken.None));
        var invalid = Assert.ThrowsAsync<AiApplicationException>(() => _handler.HandleAsync(Guid.Empty, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(notFound?.ErrorCode, Is.EqualTo(AiErrorCodes.ProviderNotFound));
            Assert.That(invalid?.StatusCode, Is.EqualTo(400));
        });
    }

    [Test]
    public async Task HandleAllChecksEveryProviderAndKeepsInactiveOnesInTheList()
    {
        var first = _store.AddProvider("A");
        var second = _store.AddProvider("B", status: "INACTIVE");

        var results = await _handler.HandleAllAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(results.Select(item => item.Provider.Id), Is.EquivalentTo(new[] { first.Id, second.Id }));
            Assert.That(results.Single(item => item.Provider.Id == first.Id).HealthStatus, Is.EqualTo("ONLINE"));
            Assert.That(results.Single(item => item.Provider.Id == second.Id).HealthStatus, Is.EqualTo("DISABLED"));
            Assert.That(_probe.Calls, Has.Count.EqualTo(1));
        });
    }

    private sealed class ThrowingSecretProtector : ISecretProtector
    {
        public string Protect(string plainText) => plainText;

        public string Unprotect(string protectedText) => throw new System.Security.Cryptography.CryptographicException("bad");

        public string Mask(string plainText) => plainText;
    }
}
