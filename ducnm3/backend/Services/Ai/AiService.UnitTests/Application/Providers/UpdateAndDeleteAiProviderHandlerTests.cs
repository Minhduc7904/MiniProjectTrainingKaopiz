using AiService.Application.Common.Errors;
using AiService.Application.UseCases.Providers.Delete;
using AiService.Application.UseCases.Providers.Update;
using AiService.UnitTests.Support;

namespace AiService.UnitTests.Application.Providers;

public sealed class UpdateAndDeleteAiProviderHandlerTests
{
    private InMemoryAiStore _store = null!;
    private UpdateAiProviderHandler _updateHandler = null!;
    private DeleteAiProviderHandler _deleteHandler = null!;

    [SetUp]
    public void SetUp()
    {
        _store = new InMemoryAiStore();
        _updateHandler = new UpdateAiProviderHandler(_store.ProviderRepository, new FakeSecretProtector());
        _deleteHandler = new DeleteAiProviderHandler(_store.ProviderRepository);
    }

    [Test]
    public async Task UpdateKeepsStoredKeyWhenApiKeyIsBlank()
    {
        var provider = _store.AddProvider();

        var result = await _updateHandler.HandleAsync(
            new UpdateAiProviderCommand(provider.Id, "Renamed", "OPENAI", null, "   ", "INACTIVE"),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Renamed"));
            Assert.That(result.Status, Is.EqualTo("INACTIVE"));
            Assert.That(_store.Providers[provider.Id].ApiKeyEncrypted, Is.EqualTo("enc:sk-original-abcd"));
            Assert.That(result.ApiKeyHint, Is.EqualTo("****abcd"));
        });
    }

    [Test]
    public async Task UpdateReplacesStoredKeyWhenNewApiKeyIsProvided()
    {
        var provider = _store.AddProvider();

        var result = await _updateHandler.HandleAsync(
            new UpdateAiProviderCommand(provider.Id, provider.Name, "OPENAI", null, "sk-new-key-9999", "ACTIVE"),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_store.Providers[provider.Id].ApiKeyEncrypted, Is.EqualTo("enc:sk-new-key-9999"));
            Assert.That(result.ApiKeyHint, Is.EqualTo("****9999"));
        });
    }

    [Test]
    public void UpdateRejectsMissingProviderAndNameOfAnotherProvider()
    {
        var first = _store.AddProvider("First");
        _store.AddProvider("Second");

        var notFound = Assert.ThrowsAsync<AiApplicationException>(() => _updateHandler.HandleAsync(
            new UpdateAiProviderCommand(Guid.NewGuid(), "Name", "OPENAI", null, null, "ACTIVE"), CancellationToken.None));
        var conflict = Assert.ThrowsAsync<AiApplicationException>(() => _updateHandler.HandleAsync(
            new UpdateAiProviderCommand(first.Id, "second", "OPENAI", null, null, "ACTIVE"), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(notFound!.ErrorCode, Is.EqualTo(AiErrorCodes.ProviderNotFound));
            Assert.That(conflict!.ErrorCode, Is.EqualTo(AiErrorCodes.ProviderNameConflict));
        });
    }

    [Test]
    public void UpdateRequiresStatus()
    {
        var provider = _store.AddProvider();

        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _updateHandler.HandleAsync(
            new UpdateAiProviderCommand(provider.Id, provider.Name, "OPENAI", null, null, null), CancellationToken.None));

        Assert.That(exception!.Details!.Select(detail => detail.Field), Does.Contain("status"));
    }

    [Test]
    public async Task DeleteRemovesProviderWithoutModels()
    {
        var provider = _store.AddProvider();

        await _deleteHandler.HandleAsync(provider.Id, CancellationToken.None);

        Assert.That(_store.Providers, Is.Empty);
    }

    [Test]
    public void DeleteRejectsProviderThatStillHasModels()
    {
        var provider = _store.AddProvider();
        _store.AddModel(provider);

        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _deleteHandler.HandleAsync(provider.Id, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(409));
            Assert.That(exception.ErrorCode, Is.EqualTo(AiErrorCodes.ProviderInUse));
            Assert.That(_store.Providers, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void DeleteRejectsEmptyIdAndMissingProvider()
    {
        var invalid = Assert.ThrowsAsync<AiApplicationException>(() => _deleteHandler.HandleAsync(Guid.Empty, CancellationToken.None));
        var missing = Assert.ThrowsAsync<AiApplicationException>(() => _deleteHandler.HandleAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(invalid!.StatusCode, Is.EqualTo(400));
            Assert.That(missing!.StatusCode, Is.EqualTo(404));
        });
    }
}
