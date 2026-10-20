using AiService.Application.Common.Errors;
using AiService.Application.UseCases.Bindings.Delete;
using AiService.Application.UseCases.Bindings.Upsert;
using AiService.UnitTests.Support;

namespace AiService.UnitTests.Application.Bindings;

public sealed class UpsertAiServiceBindingHandlerTests
{
    private InMemoryAiStore _store = null!;
    private UpsertAiServiceBindingHandler _upsertHandler = null!;
    private DeleteAiServiceBindingHandler _deleteHandler = null!;

    [SetUp]
    public void SetUp()
    {
        _store = new InMemoryAiStore();
        _upsertHandler = new UpsertAiServiceBindingHandler(_store.BindingRepository, _store.ModelRepository, _store.ProviderRepository);
        _deleteHandler = new DeleteAiServiceBindingHandler(_store.BindingRepository);
    }

    [Test]
    public async Task UpsertCreatesThenReplacesBindingForTheSameService()
    {
        var provider = _store.AddProvider();
        var first = _store.AddModel(provider, "gpt-4o");
        var second = _store.AddModel(provider, "gpt-4o-mini");

        var created = await _upsertHandler.HandleAsync(new UpsertAiServiceBindingCommand(" Course-Service ", first.Id), CancellationToken.None);
        var replaced = await _upsertHandler.HandleAsync(new UpsertAiServiceBindingCommand("course-service", second.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(created.Created, Is.True);
            Assert.That(created.Binding.ServiceName, Is.EqualTo("course-service"));
            Assert.That(replaced.Created, Is.False);
            Assert.That(replaced.Binding.Id, Is.EqualTo(created.Binding.Id));
            Assert.That(replaced.Binding.ModelId, Is.EqualTo(second.Id));
            Assert.That(_store.Bindings, Has.Count.EqualTo(1));
        });
    }

    [TestCase("ai-service")]
    [TestCase("lms-api-gateway")]
    [TestCase("unknown-service")]
    [TestCase("")]
    public void UpsertRejectsServicesThatCannotUseAi(string serviceName)
    {
        var provider = _store.AddProvider();
        var model = _store.AddModel(provider);

        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _upsertHandler.HandleAsync(
            new UpsertAiServiceBindingCommand(serviceName, model.Id), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(exception.Details!.Select(detail => detail.Field), Does.Contain("serviceName"));
        });
    }

    [Test]
    public void UpsertRejectsMissingModelAndEmptyModelId()
    {
        var missing = Assert.ThrowsAsync<AiApplicationException>(() => _upsertHandler.HandleAsync(
            new UpsertAiServiceBindingCommand("course-service", Guid.NewGuid()), CancellationToken.None));
        var empty = Assert.ThrowsAsync<AiApplicationException>(() => _upsertHandler.HandleAsync(
            new UpsertAiServiceBindingCommand("course-service", null), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(missing!.ErrorCode, Is.EqualTo(AiErrorCodes.ModelNotFound));
            Assert.That(empty!.Details!.Select(detail => detail.Field), Does.Contain("modelId"));
        });
    }

    [Test]
    public void UpsertRejectsInactiveModelOrInactiveProvider()
    {
        var activeProvider = _store.AddProvider("Active provider");
        var inactiveProvider = _store.AddProvider("Inactive provider", status: "INACTIVE");
        var inactiveModel = _store.AddModel(activeProvider, "old-model", status: "INACTIVE");
        var modelOfInactiveProvider = _store.AddModel(inactiveProvider, "gpt-4o");

        var byModel = Assert.ThrowsAsync<AiApplicationException>(() => _upsertHandler.HandleAsync(
            new UpsertAiServiceBindingCommand("course-service", inactiveModel.Id), CancellationToken.None));
        var byProvider = Assert.ThrowsAsync<AiApplicationException>(() => _upsertHandler.HandleAsync(
            new UpsertAiServiceBindingCommand("course-service", modelOfInactiveProvider.Id), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(byModel!.ErrorCode, Is.EqualTo(AiErrorCodes.ReferenceInactive));
            Assert.That(byProvider!.ErrorCode, Is.EqualTo(AiErrorCodes.ReferenceInactive));
            Assert.That(_store.Bindings, Is.Empty);
        });
    }

    [Test]
    public async Task DeleteRemovesBindingAndRejectsMissingOne()
    {
        var model = _store.AddModel(_store.AddProvider());
        await _upsertHandler.HandleAsync(new UpsertAiServiceBindingCommand("media-service", model.Id), CancellationToken.None);

        await _deleteHandler.HandleAsync("media-service", CancellationToken.None);
        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _deleteHandler.HandleAsync("media-service", CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(_store.Bindings, Is.Empty);
            Assert.That(exception!.ErrorCode, Is.EqualTo(AiErrorCodes.BindingNotFound));
        });
    }
}
