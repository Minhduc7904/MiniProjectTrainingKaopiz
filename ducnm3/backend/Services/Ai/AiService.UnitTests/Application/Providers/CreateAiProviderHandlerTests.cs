using AiService.Application.Common.Errors;
using AiService.Application.UseCases.Providers.Create;
using AiService.UnitTests.Support;

namespace AiService.UnitTests.Application.Providers;

public sealed class CreateAiProviderHandlerTests
{
    private InMemoryAiStore _store = null!;
    private CreateAiProviderHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _store = new InMemoryAiStore();
        _handler = new CreateAiProviderHandler(_store.ProviderRepository, new FakeSecretProtector());
    }

    [Test]
    public async Task HandleAsyncStoresEncryptedKeyAndReturnsOnlyHint()
    {
        var result = await _handler.HandleAsync(
            new CreateAiProviderCommand("  OpenAI main  ", "openai", null, "sk-secret-value-1234", null),
            CancellationToken.None);

        var stored = _store.Providers[result.Id];
        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("OpenAI main"));
            Assert.That(result.ProviderType, Is.EqualTo("OPENAI"));
            Assert.That(result.BaseUrl, Is.EqualTo("https://api.openai.com/v1"));
            Assert.That(result.Status, Is.EqualTo("ACTIVE"));
            Assert.That(result.ApiKeyHint, Is.EqualTo("****1234"));
            Assert.That(stored.ApiKeyEncrypted, Is.EqualTo(FakeSecretProtector.Prefix + "sk-secret-value-1234"));
            Assert.That(result.ApiKeyHint, Does.Not.Contain("secret"));
        });
    }

    [Test]
    public async Task HandleAsyncUsesExplicitBaseUrlForCustomProvider()
    {
        var result = await _handler.HandleAsync(
            new CreateAiProviderCommand("Local LLM", "CUSTOM", " http://localhost:11434/v1 ", "local-key-0000", "inactive"),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.BaseUrl, Is.EqualTo("http://localhost:11434/v1"));
            Assert.That(result.Status, Is.EqualTo("INACTIVE"));
        });
    }

    [TestCase(null, "OPENAI", null, "sk-key-12345", "name")]
    [TestCase("a", "OPENAI", null, "sk-key-12345", "name")]
    [TestCase("Valid", "UNKNOWN", null, "sk-key-12345", "providerType")]
    [TestCase("Valid", "CUSTOM", null, "sk-key-12345", "baseUrl")]
    [TestCase("Valid", "OPENAI", "ftp://example.com", "sk-key-12345", "baseUrl")]
    [TestCase("Valid", "OPENAI", "https://user:pass@example.com", "sk-key-12345", "baseUrl")]
    [TestCase("Valid", "OPENAI", null, null, "apiKey")]
    [TestCase("Valid", "OPENAI", null, "key with space", "apiKey")]
    public void HandleAsyncRejectsInvalidInput(string? name, string? providerType, string? baseUrl, string? apiKey, string expectedField)
    {
        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _handler.HandleAsync(
            new CreateAiProviderCommand(name, providerType, baseUrl, apiKey, null),
            CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(exception.Details!.Select(detail => detail.Field), Does.Contain(expectedField));
            Assert.That(_store.Providers, Is.Empty);
        });
    }

    [Test]
    public void HandleAsyncRejectsDuplicateNameCaseInsensitively()
    {
        _store.AddProvider("OpenAI main");

        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _handler.HandleAsync(
            new CreateAiProviderCommand("openai MAIN", "OPENAI", null, "sk-key-12345", null),
            CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(409));
            Assert.That(exception.ErrorCode, Is.EqualTo(AiErrorCodes.ProviderNameConflict));
        });
    }
}
