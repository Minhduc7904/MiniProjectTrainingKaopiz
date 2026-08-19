// File: backend/Services/Media/MediaService.UnitTests/Application/UseCases/Media/DirectUpload/CreateIntent/CreateUploadIntentHandlerTests.cs
// Mục đích: Kiểm thử handler CreateUploadIntentHandlerTests và các nhánh nghiệp vụ liên quan.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Application.Services.Storage;
using MediaService.Application.UseCases.Media.DirectUpload.CreateIntent;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using MediaService.UnitTests.TestDoubles;

namespace MediaService.UnitTests.Application.UseCases.Media.DirectUpload.CreateIntent;

public sealed class CreateUploadIntentHandlerTests
{
    [Test]
    public async Task HandleAsyncValidImagePersistsPendingDraftWithDeclaredChecksum()
    {
        var repository = new StubMediaRepository();
        var policyProvider = new StubPolicyProvider();
        var handler = new CreateUploadIntentHandler(
            new StubActorValidationService(),
            new StubStorageLocationAllocator(),
            policyProvider,
            repository,
            new MediaUploadOptions());

        var result = await handler.HandleAsync(
            new CreateUploadIntentCommand(
                "photo.png", "IMAGE", "image/png", 3,
                new string('a', 64),
                new ActorReference("student", Guid.Parse("11111111-1111-1111-1111-111111111111"))),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(repository.Pending, Is.Not.Null);
            Assert.That(repository.Pending!.ExpectedChecksumSha256, Is.EqualTo(new string('a', 64)));
            Assert.That(repository.Pending.IsDraft, Is.True);
            Assert.That(repository.Pending.DraftedAtUtc, Is.Null);
            Assert.That(result.Status, Is.EqualTo("PENDING"));
            Assert.That(result.FormFields, Contains.Key("policy"));
        });
    }

    [TestCase("ABC")]
    [TestCase("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void HandleAsyncInvalidDeclaredChecksumRejectsBeforePersistence(string checksum)
    {
        var repository = new StubMediaRepository();
        var handler = new CreateUploadIntentHandler(
            new StubActorValidationService(),
            new StubStorageLocationAllocator(),
            new StubPolicyProvider(),
            repository,
            new MediaUploadOptions());

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() =>
            handler.HandleAsync(
                new CreateUploadIntentCommand(
                    "photo.png", "IMAGE", "image/png", 3, checksum,
                    new ActorReference("STUDENT", Guid.NewGuid())),
                TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo("INVALID_MEDIA"));
            Assert.That(repository.Pending, Is.Null);
        });
    }

    [TestCase("../photo.png", "IMAGE", "image/png", 3L, "INVALID_MEDIA")]
    [TestCase("photo", "IMAGE", "image/png", 3L, "INVALID_MEDIA")]
    [TestCase("photo.png", "UNKNOWN", "image/png", 3L, "INVALID_MEDIA")]
    [TestCase("photo.png", "VIDEO", "image/png", 3L, "UNSUPPORTED_MEDIA_TYPE")]
    [TestCase("photo.png", "IMAGE", "image/png", 0L, "INVALID_MEDIA")]
    [TestCase("photo.png", "IMAGE", "image/png", 10485761L, "PAYLOAD_TOO_LARGE")]
    public void HandleAsyncInvalidDeclarationRejectsBeforePersistence(
        string fileName,
        string mediaType,
        string contentType,
        long sizeBytes,
        string errorCode)
    {
        var repository = new StubMediaRepository();
        var handler = CreateHandler(repository, new StubActorValidationService());

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() =>
            handler.HandleAsync(
                new CreateUploadIntentCommand(
                    fileName, mediaType, contentType, sizeBytes, new string('a', 64),
                    new ActorReference("STUDENT", Guid.NewGuid())),
                TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo(errorCode));
            Assert.That(repository.Pending, Is.Null);
        });
    }

    [Test]
    public void HandleAsyncActorValidationFailsDoesNotPersistIntent()
    {
        var repository = new StubMediaRepository();
        var handler = CreateHandler(repository, new RejectActorValidationService());

        var exception = Assert.ThrowsAsync<MediaService.Application.Common.Errors.MediaApplicationException>(() =>
            handler.HandleAsync(
                ValidCommand(),
                TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo("ACTOR_NOT_FOUND"));
            Assert.That(repository.Pending, Is.Null);
        });
    }

    [Test]
    public async Task HandleAsyncRepeatedRequestCreatesDistinctMediaIds()
    {
        var firstRepository = new StubMediaRepository();
        var secondRepository = new StubMediaRepository();
        var first = await CreateHandler(firstRepository, new StubActorValidationService())
            .HandleAsync(ValidCommand(), TestContext.CurrentContext.CancellationToken);
        var second = await CreateHandler(secondRepository, new StubActorValidationService())
            .HandleAsync(ValidCommand(), TestContext.CurrentContext.CancellationToken);

        Assert.That(second.MediaId, Is.Not.EqualTo(first.MediaId));
    }

    private static CreateUploadIntentHandler CreateHandler(
        StubMediaRepository repository,
        IActorValidationService actorValidationService) =>
        new(
            actorValidationService,
            new StubStorageLocationAllocator(),
            new StubPolicyProvider(),
            repository,
            new MediaUploadOptions());

    private static CreateUploadIntentCommand ValidCommand() =>
        new(
            "photo.png", "IMAGE", "image/png", 3, new string('a', 64),
            new ActorReference("STUDENT", Guid.Parse("11111111-1111-1111-1111-111111111111")));

    private sealed class RejectActorValidationService : IActorValidationService
    {
        public Task<ActorReference> ValidateAsync(
            ActorReference actor,
            CancellationToken cancellationToken) =>
            throw MediaService.Application.Common.Errors.MediaErrors.ActorNotFound();
    }

    private sealed class StubPolicyProvider : IStorageUploadPolicyProvider
    {
        public Task<StorageUploadPolicy> CreateAsync(
            StorageUploadPolicyRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new StorageUploadPolicy(
                new Uri("http://localhost:9000/images"),
                new Dictionary<string, string> { ["policy"] = "signed" },
                DateTime.UtcNow.AddMinutes(15)));
    }
}
