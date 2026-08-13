using System.Security.Cryptography;
using BuildingBlocks.DatabaseMigration;
using MediaService.Application.Actors;
using MediaService.Application.Upload;
using MediaService.Application.Usages;
using MediaService.Domain;
using MediaService.Infrastructure.Persistence;
using MediaService.Infrastructure.Storage.Minio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Testcontainers.Minio;
using Testcontainers.MySql;

namespace MediaService.IntegrationTests.Flows;

[NonParallelizable]
public sealed class MediaUploadUsageFlowTests
{
    private readonly MySqlContainer mysql = new MySqlBuilder("mysql:8.4")
        .WithDatabase("media_flow_tests")
        .WithUsername("media_test")
        .WithPassword("media_test_password")
        .Build();
    private readonly MinioContainer minio = new MinioBuilder(
        "minio/minio:RELEASE.2025-09-07T16-13-09Z").Build();
    private IMinioClient minioClient = null!;
    private MinioStorageService storage = null!;
    private MinioStorageLocationAllocator allocator = null!;

    [OneTimeSetUp]
    public async Task StartDependenciesAsync()
    {
        await mysql.StartAsync();
        await minio.StartAsync();
        await SqlMigrationRunner.ApplyAsync(
            new SqlMigrationRunnerOptions(
                "media-flow-tests",
                mysql.GetConnectionString(),
                Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "Database",
                    "Migrations")),
            TestContext.Progress.WriteLine);

        var endpoint = new Uri(minio.GetConnectionString());
        var options = CreateStorageOptions(endpoint.Authority);
        minioClient = new MinioClient()
            .WithEndpoint(endpoint.Host, endpoint.Port)
            .WithCredentials(minio.GetAccessKey(), minio.GetSecretKey())
            .WithSSL(false)
            .Build();
        foreach (var bucket in options.GetBuckets())
        {
            await minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucket));
        }

        var wrappedOptions = Options.Create(options);
        var keyGenerator = new MinioObjectKeyGenerator(TimeProvider.System);
        allocator = new MinioStorageLocationAllocator(
            wrappedOptions,
            keyGenerator);
        storage = new MinioStorageService(
            minioClient,
            wrappedOptions,
            NullLogger<MinioStorageService>.Instance);
    }

    [OneTimeTearDown]
    public async Task StopDependenciesAsync()
    {
        minioClient?.Dispose();
        await minio.DisposeAsync();
        await mysql.DisposeAsync();
    }

    [Test]
    public async Task UploadPersistsReadyChecksumAndUsageReplacesStudentAvatar()
    {
        var dbOptions = new DbContextOptionsBuilder<MediaDbContext>()
            .UseMySql(
                mysql.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        await using var dbContext = new MediaDbContext(dbOptions);
        var repository = new EfMediaRepository(dbContext, TimeProvider.System);
        var studentLookup = new ExistingStudentLookup();
        var actorValidation = new ActorValidationService(
            [new StudentActorValidator(studentLookup)]);
        var uploadHandler = new UploadMediaHandler(
            actorValidation,
            allocator,
            storage,
            repository,
            new MediaUploadOptions(),
            TimeProvider.System,
            NullLogger<UploadMediaHandler>.Instance);
        var actorId = Guid.NewGuid();
        var firstBytes = new byte[] { 1, 2, 3, 4 };
        await using var firstContent = new MemoryStream(firstBytes);

        var firstMedia = await uploadHandler.HandleAsync(
            CreateUploadCommand(firstContent, actorId, "first.png"),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();
        var storedMedia = await dbContext.MediaObjects
            .AsNoTracking()
            .SingleAsync(item => item.Id == firstMedia.Id);

        Assert.Multiple(() =>
        {
            Assert.That(storedMedia.Status, Is.EqualTo(MediaObjectStatuses.Ready));
            Assert.That(
                storedMedia.ChecksumSha256,
                Is.EqualTo(Convert.ToHexString(SHA256.HashData(firstBytes)).ToLowerInvariant()));
            Assert.That(storedMedia.UploadedByType, Is.EqualTo(ActorTypes.Student));
        });
        Assert.That(
            await storage.ExistsAsync(
                new(storedMedia.Bucket, storedMedia.ObjectKey),
                CancellationToken.None),
            Is.True);

        var usageHandler = new CreateMediaUsageHandler(
            actorValidation,
            studentLookup,
            repository);
        var ownerId = actorId;
        await usageHandler.HandleAsync(
            CreateUsageCommand(firstMedia.Id, ownerId, actorId),
            CancellationToken.None);

        await using var secondContent = new MemoryStream(new byte[] { 5, 6, 7 });
        var secondMedia = await uploadHandler.HandleAsync(
            CreateUploadCommand(secondContent, actorId, "second.png"),
            CancellationToken.None);
        await usageHandler.HandleAsync(
            CreateUsageCommand(secondMedia.Id, ownerId, actorId),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var usages = await dbContext.MediaUsages
            .AsNoTracking()
            .Where(item => item.OwnerId == ownerId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(usages, Has.Count.EqualTo(2));
            Assert.That(usages.Count(item => item.DeletedAt is null), Is.EqualTo(1));
            Assert.That(usages.Single(item => item.DeletedAt is null).MediaId, Is.EqualTo(secondMedia.Id));
            Assert.That(usages.All(item => item.CreatedByType == ActorTypes.Student), Is.True);
        });
    }

    private static UploadMediaCommand CreateUploadCommand(
        Stream content,
        Guid actorId,
        string fileName) =>
        new(
            MediaTypes.Image,
            "image/png",
            fileName,
            content,
            content.Length,
            new ActorReference(ActorTypes.Student, actorId));

    private static CreateMediaUsageCommand CreateUsageCommand(
        Guid mediaId,
        Guid ownerId,
        Guid actorId) =>
        new(
            mediaId,
            MediaOwnerServices.Student,
            MediaOwnerTypes.StudentAvatar,
            ownerId,
            MediaUsageTypes.Avatar,
            0,
            new ActorReference(ActorTypes.Student, actorId));

    private static MinioStorageOptions CreateStorageOptions(string endpoint) =>
        new()
        {
            Endpoint = endpoint,
            AccessKey = "integration",
            SecretKey = "integration-secret",
            UseSsl = false,
            ImageBucket = "flow-images",
            VideoBucket = "flow-videos",
            DocumentBucket = "flow-documents",
            AudioBucket = "flow-audios",
            OtherBucket = "flow-other",
        };

    private sealed class ExistingStudentLookup : IStudentLookup
    {
        public Task<StudentLookupResult?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult<StudentLookupResult?>(
                new(
                    studentId,
                    "student@example.com",
                    "Student",
                    "ACTIVE"));
    }
}
