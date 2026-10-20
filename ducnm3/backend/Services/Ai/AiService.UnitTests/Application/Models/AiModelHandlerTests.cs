using AiService.Application.Common.Errors;
using AiService.Application.UseCases.Models.Create;
using AiService.Application.UseCases.Models.Delete;
using AiService.Application.UseCases.Models.Update;
using AiService.UnitTests.Support;

namespace AiService.UnitTests.Application.Models;

public sealed class AiModelHandlerTests
{
    private static readonly string[] ExpectedInvalidFields = ["providerId", "modelName"];

    private InMemoryAiStore _store = null!;
    private CreateAiModelHandler _createHandler = null!;
    private UpdateAiModelHandler _updateHandler = null!;
    private DeleteAiModelHandler _deleteHandler = null!;

    [SetUp]
    public void SetUp()
    {
        _store = new InMemoryAiStore();
        _createHandler = new CreateAiModelHandler(_store.ModelRepository, _store.ProviderRepository);
        _updateHandler = new UpdateAiModelHandler(_store.ModelRepository);
        _deleteHandler = new DeleteAiModelHandler(_store.ModelRepository);
    }

    [Test]
    public async Task CreateDefaultsDisplayNameToModelNameAndStatusToActive()
    {
        var provider = _store.AddProvider();

        var result = await _createHandler.HandleAsync(
            new CreateAiModelCommand(provider.Id, " gpt-4o ", null, null),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.ModelName, Is.EqualTo("gpt-4o"));
            Assert.That(result.DisplayName, Is.EqualTo("gpt-4o"));
            Assert.That(result.Status, Is.EqualTo("ACTIVE"));
            Assert.That(result.ProviderName, Is.EqualTo(provider.Name));
        });
    }

    [Test]
    public void CreateRejectsMissingProviderDuplicateModelAndInvalidName()
    {
        var provider = _store.AddProvider();
        _store.AddModel(provider, "gpt-4o");

        var missingProvider = Assert.ThrowsAsync<AiApplicationException>(() => _createHandler.HandleAsync(
            new CreateAiModelCommand(Guid.NewGuid(), "gpt-4o", null, null), CancellationToken.None));
        var duplicate = Assert.ThrowsAsync<AiApplicationException>(() => _createHandler.HandleAsync(
            new CreateAiModelCommand(provider.Id, "GPT-4O", null, null), CancellationToken.None));
        var invalid = Assert.ThrowsAsync<AiApplicationException>(() => _createHandler.HandleAsync(
            new CreateAiModelCommand(null, "has space", null, null), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(missingProvider!.ErrorCode, Is.EqualTo(AiErrorCodes.ProviderNotFound));
            Assert.That(duplicate!.ErrorCode, Is.EqualTo(AiErrorCodes.ModelConflict));
            Assert.That(invalid!.Details!.Select(detail => detail.Field), Is.EquivalentTo(ExpectedInvalidFields));
        });
    }

    [Test]
    public async Task UpdateChangesNameDisplayNameAndStatusButKeepsProvider()
    {
        var provider = _store.AddProvider();
        var model = _store.AddModel(provider, "gpt-4o");

        var result = await _updateHandler.HandleAsync(
            new UpdateAiModelCommand(model.Id, "gpt-4o-mini", "Mini", "INACTIVE"),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.ProviderId, Is.EqualTo(provider.Id));
            Assert.That(result.ModelName, Is.EqualTo("gpt-4o-mini"));
            Assert.That(result.DisplayName, Is.EqualTo("Mini"));
            Assert.That(result.Status, Is.EqualTo("INACTIVE"));
        });
    }

    [Test]
    public void UpdateRejectsNameUsedByAnotherModelOfSameProvider()
    {
        var provider = _store.AddProvider();
        _store.AddModel(provider, "gpt-4o");
        var other = _store.AddModel(provider, "gpt-4o-mini");

        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _updateHandler.HandleAsync(
            new UpdateAiModelCommand(other.Id, "gpt-4o", null, "ACTIVE"), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(AiErrorCodes.ModelConflict));
    }

    [Test]
    public async Task DeleteRemovesUnboundModelAndRejectsBoundModel()
    {
        var provider = _store.AddProvider();
        var free = _store.AddModel(provider, "free");
        var bound = _store.AddModel(provider, "bound");
        await _store.BindingRepository.UpsertAsync("course-service", bound.Id, CancellationToken.None);

        await _deleteHandler.HandleAsync(free.Id, CancellationToken.None);
        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _deleteHandler.HandleAsync(bound.Id, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(_store.Models.Keys, Is.EquivalentTo(new[] { bound.Id }));
            Assert.That(exception!.ErrorCode, Is.EqualTo(AiErrorCodes.ModelInUse));
        });
    }
}
