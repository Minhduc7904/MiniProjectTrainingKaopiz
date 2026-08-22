// File: backend/Services/Media/MediaService.IntegrationTests/Flows/MediaUploadUsageFlowTests.cs
// Mục đích: Kiểm thử flow tích hợp upload Media rồi gắn Media Usage với các dependency thực để bảo vệ vòng đời draft và ready.

using System.Globalization;
using System.Security.Cryptography;
using BuildingBlocks.Contracts.Students;
using BuildingBlocks.DatabaseMigration;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Application;
using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Application.Services.Storage;
using MediaService.Application.Services.Students;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Application.UseCases.MediaUsages.Create;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;
using MediaService.Infrastructure.Persistence;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Infrastructure.Persistence.Repositories;
using MediaService.Infrastructure.Persistence.Transactions;
using MediaService.Infrastructure.Storage.Minio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using MySqlConnector;
using Testcontainers.Minio;
using Testcontainers.MySql;

namespace MediaService.IntegrationTests.Flows;

[NonParallelizable]
public sealed class MediaUploadUsageFlowTests
{
    private static readonly Guid LegacyUsedMediaId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LegacyUnusedMediaId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid LegacyPendingMediaId =
        Guid.Parse("55555555-5555-5555-5555-555555555555");
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
        await ApplyMigrationsWithLegacyDraftBackfillDataAsync();

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
            new MinioInternalClient(minioClient),
            wrappedOptions,
            NullLogger<MinioStorageService>.Instance);
    }

    [Test]
    public async Task V005BackfillsExistingMediaDraftStateAndCreatesIndex()
    {
        var dbOptions = new DbContextOptionsBuilder<MediaDbContext>()
            .UseMySql(
                mysql.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        await using var dbContext = new MediaDbContext(dbOptions);
        var media = await dbContext.MediaObjects
            .AsNoTracking()
            .Where(item =>
                item.Id == LegacyUsedMediaId ||
                item.Id == LegacyUnusedMediaId ||
                item.Id == LegacyPendingMediaId)
            .OrderBy(item => item.Id)
            .ToListAsync();

        await using var connection = new MySqlConnection(mysql.GetConnectionString());
        await connection.OpenAsync();
        await using var indexCommand = new MySqlCommand(
            """
            SELECT COUNT(*)
            FROM information_schema.statistics
            WHERE table_schema = DATABASE()
              AND table_name = 'media_objects'
              AND index_name = 'ix_media_objects_draft_cleanup';
            """,
            connection);
        var indexColumnCount = Convert.ToInt32(
            await indexCommand.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);

        var used = media.Single(item => item.Id == LegacyUsedMediaId);
        var unused = media.Single(item => item.Id == LegacyUnusedMediaId);
        var pending = media.Single(item => item.Id == LegacyPendingMediaId);
        Assert.Multiple(() =>
        {
            Assert.That(used.IsDraft, Is.False);
            Assert.That(used.DraftedAt, Is.Null);
            Assert.That(unused.IsDraft, Is.True);
            Assert.That(
                unused.DraftedAt,
                Is.EqualTo(new DateTime(2026, 8, 17, 2, 0, 0, DateTimeKind.Utc)));
            Assert.That(pending.Status, Is.EqualTo(MediaObjectStatuses.Pending));
            Assert.That(pending.CompletedAt, Is.Null);
            Assert.That(pending.IsDraft, Is.True);
            Assert.That(
                pending.DraftedAt,
                Is.EqualTo(new DateTime(2026, 8, 16, 4, 0, 0, DateTimeKind.Utc)));
            Assert.That(indexColumnCount, Is.EqualTo(4));
        });
    }

    [Test]
    public async Task GetActiveUsageIdsByOwnersAsyncMultipleOwnerScopesReturnsOnlyExactActiveUsageIds()
    {
        var dbOptions = new DbContextOptionsBuilder<MediaDbContext>()
            .UseMySql(
                mysql.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var courseUsageId = Guid.NewGuid();
        var lessonUsageId = Guid.NewGuid();
        var unrelatedUsageId = Guid.NewGuid();

        await using (var setupContext = new MediaDbContext(dbOptions))
        {
            setupContext.MediaUsages.AddRange(
                CreateUsage(courseUsageId, courseId, "COURSE", "COURSE_THUMBNAIL"),
                CreateUsage(lessonUsageId, lessonId, "COURSE", "LESSON_ATTACHMENT"),
                CreateUsage(unrelatedUsageId, courseId, "STUDENT", "STUDENT_AVATAR"));
            await setupContext.SaveChangesAsync();
        }

        await using var queryContext = new MediaDbContext(dbOptions);
        var repository = new EfMediaUsageRepository(queryContext, TimeProvider.System);

        var result = await repository.GetActiveUsageIdsByOwnersAsync(
            [
                new MediaUsageOwnerScope("COURSE", "COURSE_THUMBNAIL", courseId),
                new MediaUsageOwnerScope("COURSE", "LESSON_ATTACHMENT", lessonId),
            ],
            TestContext.CurrentContext.CancellationToken);

        Assert.That(result, Is.EquivalentTo([courseUsageId, lessonUsageId]));
    }

    [Test]
    public async Task ReplaceCourseThumbnailAsyncReadyOriginalImageCreatesUsageForOriginalMedia()
    {
        var dbOptions = new DbContextOptionsBuilder<MediaDbContext>()
            .UseMySql(
                mysql.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        var adminId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var originalMediaId = Guid.NewGuid();

        await using (var setupContext = new MediaDbContext(dbOptions))
        {
            setupContext.MediaObjects.Add(CreateReadyOriginalImage(originalMediaId, adminId));
            await setupContext.SaveChangesAsync();
        }

        await using var usageContext = new MediaDbContext(dbOptions);
        var repository = new EfMediaUsageRepository(usageContext, TimeProvider.System);

        var usage = await repository.ReplaceCourseThumbnailAsync(
            new CreateMediaUsageRecord(
                Guid.NewGuid(),
                originalMediaId,
                MediaOwnerServices.Course,
                MediaOwnerTypes.CourseThumbnail,
                courseId,
                MediaUsageTypes.Thumbnail,
                0,
                new ActorReference(ActorTypes.Admin, adminId)),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(usage.MediaId, Is.EqualTo(originalMediaId));
            Assert.That(usage.OwnerType, Is.EqualTo(MediaOwnerTypes.CourseThumbnail));
        });
    }

    [Test]
    public async Task ListByActorAsyncReturnsOnlyOriginalMedia()
    {
        var dbOptions = new DbContextOptionsBuilder<MediaDbContext>()
            .UseMySql(
                mysql.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        var adminId = Guid.NewGuid();
        var originalMediaId = Guid.NewGuid();
        var derivativeMediaId = Guid.NewGuid();

        await using (var setupContext = new MediaDbContext(dbOptions))
        {
            setupContext.MediaObjects.Add(CreateReadyOriginalImage(originalMediaId, adminId));
            setupContext.MediaObjects.Add(new MediaService.Infrastructure.Persistence.Scaffolded.MediaObject
            {
                Id = derivativeMediaId,
                SourceMediaId = originalMediaId,
                DerivationType = MediaDerivationTypes.Thumbnail,
                Bucket = "images",
                ObjectKey = $"integration/{derivativeMediaId:N}.webp",
                MediaType = MediaTypes.Image,
                ContentType = "image/webp",
                OriginalFileName = "course-thumbnail.thumbnail.webp",
                SizeBytes = 1,
                ChecksumSha256 = new string('d', 64),
                UploadedBy = adminId,
                UploadedByType = ActorTypes.Admin,
                Status = MediaObjectStatuses.Ready,
                IsDraft = true,
                DraftedAt = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc),
                CompletedAt = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc),
            });
            await setupContext.SaveChangesAsync();
        }

        await using var queryContext = new MediaDbContext(dbOptions);
        var repository = new EfMediaRepository(
            queryContext,
            new EfMediaBackgroundJobRepository(queryContext),
            TimeProvider.System);

        var result = await repository.ListByActorAsync(
            new ActorReference(ActorTypes.Admin, adminId),
            MediaTypes.Image,
            null,
            20,
            TestContext.CurrentContext.CancellationToken);

        Assert.That(result.Select(item => item.Id), Is.EqualTo([originalMediaId]));
    }

    [Test]
    public async Task CountAsync_NewMediaObjects_ReturnsEveryPersistedRow()
    {
        var dbOptions = new DbContextOptionsBuilder<MediaDbContext>()
            .UseMySql(
                mysql.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        await using var dbContext = new MediaDbContext(dbOptions);
        var repository = new EfMediaRepository(
            dbContext,
            new EfMediaBackgroundJobRepository(dbContext),
            TimeProvider.System);
        var before = await repository.CountAsync(TestContext.CurrentContext.CancellationToken);
        dbContext.MediaObjects.AddRange(
            CreateReadyOriginalImage(Guid.NewGuid(), Guid.NewGuid()),
            CreateReadyOriginalImage(Guid.NewGuid(), Guid.NewGuid()));
        await dbContext.SaveChangesAsync(TestContext.CurrentContext.CancellationToken);

        var result = await repository.CountAsync(TestContext.CurrentContext.CancellationToken);

        Assert.That(result, Is.EqualTo(before + 2));
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
        var repository = new EfMediaRepository(
            dbContext,
            new EfMediaBackgroundJobRepository(dbContext),
            TimeProvider.System);
        var mediaUsageRepository = new EfMediaUsageRepository(dbContext, TimeProvider.System);
        var uploadFinalizer = new EfMediaUploadFinalizer(
            dbContext,
            new StubCommandSender());
        var studentLookup = new ExistingStudentLookup();
        var actorValidation = new ActorValidationService(
            [new StudentActorValidator(studentLookup)]);
        var uploadHandler = new UploadMediaHandler(
            allocator,
            storage,
            repository,
            uploadFinalizer,
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
            Assert.That(storedMedia.IsDraft, Is.True);
            Assert.That(storedMedia.DraftedAt, Is.EqualTo(storedMedia.CompletedAt));
            Assert.That(firstMedia.IsDraft, Is.True);
            Assert.That(
                firstMedia.DraftedAtUtc,
                Is.EqualTo(storedMedia.CompletedAt).Within(TimeSpan.FromMilliseconds(1)));
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

        var derivationJob = await dbContext.MediaDerivationJobs
            .AsNoTracking()
            .SingleAsync(item => item.SourceMediaId == firstMedia.Id);
        var derivationRepository = new EfMediaDerivationRepository(
            dbContext,
            new StubCommandSender(),
            mediaUsageRepository,
            TimeProvider.System);
        var work = await derivationRepository.BeginAsync(
            derivationJob.Id,
            derivationJob.SourceMediaId,
            derivationJob.DerivativeMediaId,
            CancellationToken.None);
        Assert.That(work, Is.Not.Null);
        await derivationRepository.CompleteAsync(
            derivationJob.Id,
            derivationJob.DerivativeMediaId,
            new string('a', 64),
            100,
            DateTime.UtcNow,
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();
        var generatedThumbnail = await dbContext.MediaObjects
            .AsNoTracking()
            .SingleAsync(item => item.Id == derivationJob.DerivativeMediaId);
        var thumbnailUsage = await dbContext.MediaUsages
            .AsNoTracking()
            .SingleAsync(item =>
                item.OwnerService == MediaOwnerServices.Media &&
                item.OwnerId == firstMedia.Id &&
                item.DeletedAt == null);
        Assert.Multiple(() =>
        {
            Assert.That(
                generatedThumbnail.Status,
                Is.EqualTo(MediaObjectStatuses.Ready));
            Assert.That(generatedThumbnail.ContentType, Is.EqualTo("image/webp"));
            Assert.That(thumbnailUsage.MediaId, Is.EqualTo(generatedThumbnail.Id));
        });

        var usageHandler = new CreateMediaUsageHandler(
            actorValidation,
            repository,
            mediaUsageRepository);
        var ownerId = actorId;
        await usageHandler.HandleAsync(
            CreateUsageCommand(firstMedia.Id, ownerId, actorId),
            CancellationToken.None);

        await using var secondContent = new MemoryStream(new byte[] { 5, 6, 7 });
        var secondMedia = await uploadHandler.HandleAsync(
            CreateUploadCommand(secondContent, actorId, "second.png"),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();
        var secondDerivationJob = await dbContext.MediaDerivationJobs
            .AsNoTracking()
            .SingleAsync(item => item.SourceMediaId == secondMedia.Id);
        await derivationRepository.BeginAsync(
            secondDerivationJob.Id,
            secondDerivationJob.SourceMediaId,
            secondDerivationJob.DerivativeMediaId,
            CancellationToken.None);
        await derivationRepository.CompleteAsync(
            secondDerivationJob.Id,
            secondDerivationJob.DerivativeMediaId,
            new string('b', 64),
            120,
            DateTime.UtcNow,
            CancellationToken.None);
        await usageHandler.HandleAsync(
            CreateUsageCommand(secondMedia.Id, ownerId, actorId),
            CancellationToken.None);
        await usageHandler.HandleAsync(
            CreateUsageCommand(firstMedia.Id, ownerId, actorId),
            CancellationToken.None);
        var conflict = Assert.ThrowsAsync<MediaApplicationException>(
            () => usageHandler.HandleAsync(
                CreateUsageCommand(firstMedia.Id, ownerId, actorId),
                CancellationToken.None));
        dbContext.ChangeTracker.Clear();

        var usages = await dbContext.MediaUsages
            .AsNoTracking()
            .Where(item => item.OwnerId == ownerId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync();
        var thumbnailUsages = await dbContext.MediaUsages
            .AsNoTracking()
            .Where(item =>
                item.OwnerService == MediaOwnerServices.Media &&
                item.OwnerId == firstMedia.Id)
            .ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(usages, Has.Count.EqualTo(3));
            Assert.That(usages.Count(item => item.DeletedAt is null), Is.EqualTo(1));
            Assert.That(usages.Single(item => item.DeletedAt is null).MediaId, Is.EqualTo(firstMedia.Id));
            Assert.That(usages.All(item => item.CreatedByType == ActorTypes.Student), Is.True);
            Assert.That(
                conflict!.ErrorCode,
                Is.EqualTo(MediaErrorCodes.MediaUsageConflict));
            Assert.That(thumbnailUsages, Has.Count.EqualTo(1));
            Assert.That(
                thumbnailUsages.Single(item => item.DeletedAt is null).MediaId,
                Is.EqualTo(derivationJob.DerivativeMediaId));
        });
    }

    [Test]
    public async Task V006TracksNotificationMediaUsageJobUntilExpectedCountCompletes()
    {
        var dbOptions = new DbContextOptionsBuilder<MediaDbContext>()
            .UseMySql(
                mysql.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        var jobId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        await using (var firstContext = new MediaDbContext(dbOptions))
        {
            var repository = new EfNotificationMediaUsageJobRepository(firstContext);
            await repository.StartAsync(jobId, CancellationToken.None);
            await repository.RecordSuccessAsync(jobId, 40, CancellationToken.None);
            await repository.CompleteSourceAsync(jobId, 100, CancellationToken.None);
            var processing = await repository.GetByIdAsync(jobId, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(processing?.Status, Is.EqualTo(NotificationMediaUsageJobStatuses.Processing));
                Assert.That(processing?.ProcessedUsageCount, Is.EqualTo(40));
                Assert.That(processing?.ExpectedUsageCount, Is.EqualTo(100));
            });
        }

        await using (var secondContext = new MediaDbContext(dbOptions))
        {
            var repository = new EfNotificationMediaUsageJobRepository(secondContext);
            await repository.RecordSuccessAsync(jobId, 60, CancellationToken.None);
            var completed = await repository.GetByIdAsync(jobId, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(completed?.Status, Is.EqualTo(NotificationMediaUsageJobStatuses.Completed));
                Assert.That(completed?.ProcessedUsageCount, Is.EqualTo(100));
                Assert.That(completed?.CompletedAtUtc, Is.Not.Null);
            });
        }
    }

    private static UploadMediaCommand CreateUploadCommand(
        Stream content,
        Guid actorId,
        string fileName) =>
        new(
            "image/png",
            fileName,
            content,
            content.Length,
            new ActorReference(ActorTypes.Student, actorId));

    private async Task ApplyMigrationsWithLegacyDraftBackfillDataAsync()
    {
        var migrationsDirectory = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Database",
            "Migrations");
        var preDraftMigrationsDirectory = Directory.CreateTempSubdirectory(
            "media-pre-draft-migrations-").FullName;
        try
        {
            foreach (var migration in Directory
                         .EnumerateFiles(migrationsDirectory, "V*.sql")
                         .Where(path =>
                             Path.GetFileName(path) is { } fileName &&
                             (fileName.StartsWith("V001__", StringComparison.Ordinal) ||
                              fileName.StartsWith("V002__", StringComparison.Ordinal) ||
                              fileName.StartsWith("V003__", StringComparison.Ordinal) ||
                              fileName.StartsWith("V004__", StringComparison.Ordinal))))
            {
                File.Copy(
                    migration,
                    Path.Combine(
                        preDraftMigrationsDirectory,
                        Path.GetFileName(migration)));
            }

            await SqlMigrationRunner.ApplyAsync(
                new SqlMigrationRunnerOptions(
                    "media-flow-tests-pre-draft",
                    mysql.GetConnectionString(),
                    preDraftMigrationsDirectory),
                TestContext.Progress.WriteLine);
            await InsertLegacyDraftBackfillDataAsync();
            await SqlMigrationRunner.ApplyAsync(
                new SqlMigrationRunnerOptions(
                    "media-flow-tests",
                    mysql.GetConnectionString(),
                    migrationsDirectory),
                TestContext.Progress.WriteLine);
        }
        finally
        {
            Directory.Delete(preDraftMigrationsDirectory, recursive: true);
        }
    }

    private async Task InsertLegacyDraftBackfillDataAsync()
    {
        await using var connection = new MySqlConnection(mysql.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO media_objects (
                id, bucket, object_key, media_type, content_type,
                original_file_name, size_bytes, checksum_sha256,
                uploaded_by, uploaded_by_type, status, completed_at,
                updated_at, created_at)
            VALUES
                (@usedMediaId, 'images', 'legacy/used.png', 'IMAGE', 'image/png',
                 'used.png', 1, @checksum, @actorId, 'STUDENT', 'READY',
                 '2026-08-17 01:00:00.000000', '2026-08-17 01:00:00.000000',
                 '2026-08-16 01:00:00.000000'),
                (@unusedMediaId, 'images', 'legacy/unused.png', 'IMAGE', 'image/png',
                 'unused.png', 1, @checksum, @actorId, 'STUDENT', 'READY',
                 '2026-08-17 02:00:00.000000', '2026-08-17 02:00:00.000000',
                 '2026-08-16 02:00:00.000000'),
                (@pendingMediaId, 'images', 'legacy/pending.png', 'IMAGE', 'image/png',
                 'pending.png', 1, NULL, @actorId, 'STUDENT', 'PENDING',
                 NULL, '2026-08-16 04:00:00.000000',
                 '2026-08-16 04:00:00.000000');

            INSERT INTO media_usages (
                id, media_id, owner_service, owner_type, owner_id, usage_type,
                display_order, created_by, created_by_type, created_at)
            VALUES (
                @usageId, @usedMediaId, 'STUDENT', 'STUDENT_AVATAR', @actorId,
                'AVATAR', 0, @actorId, 'STUDENT', '2026-08-17 03:00:00.000000');
            """,
            connection);
        command.Parameters.AddWithValue("@usedMediaId", LegacyUsedMediaId);
        command.Parameters.AddWithValue("@unusedMediaId", LegacyUnusedMediaId);
        command.Parameters.AddWithValue("@pendingMediaId", LegacyPendingMediaId);
        command.Parameters.AddWithValue("@usageId", Guid.Parse("33333333-3333-3333-3333-333333333333"));
        command.Parameters.AddWithValue("@actorId", Guid.Parse("44444444-4444-4444-4444-444444444444"));
        command.Parameters.AddWithValue("@checksum", new string('a', 64));
        await command.ExecuteNonQueryAsync();
    }

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

    private static MediaService.Infrastructure.Persistence.Scaffolded.MediaUsage CreateUsage(
        Guid usageId,
        Guid ownerId,
        string ownerService,
        string ownerType) =>
        new()
        {
            Id = usageId,
            MediaId = LegacyUsedMediaId,
            OwnerId = ownerId,
            OwnerService = ownerService,
            OwnerType = ownerType,
            UsageType = "ATTACHMENT",
            DisplayOrder = 0,
            CreatedBy = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            CreatedByType = ActorTypes.Student,
            CreatedAt = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc),
        };

    private static MediaService.Infrastructure.Persistence.Scaffolded.MediaObject CreateReadyOriginalImage(
        Guid mediaId,
        Guid adminId) =>
        new()
        {
            Id = mediaId,
            Bucket = "images",
            ObjectKey = $"integration/{mediaId:N}.png",
            MediaType = MediaTypes.Image,
            ContentType = "image/png",
            OriginalFileName = "course-thumbnail.png",
            SizeBytes = 1,
            ChecksumSha256 = new string('c', 64),
            UploadedBy = adminId,
            UploadedByType = ActorTypes.Admin,
            Status = MediaObjectStatuses.Ready,
            IsDraft = true,
            DraftedAt = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc),
            CompletedAt = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc),
        };

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
        public Task<StudentQueryResponse?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult<StudentQueryResponse?>(
                new(
                    studentId,
                    "student@example.com",
                    "Student",
                    "ACTIVE"));
    }

    private sealed class StubCommandSender : ICommandSender
    {
        public Task SendAsync<TCommand>(
            string destinationService,
            TCommand command,
            CancellationToken cancellationToken = default)
            where TCommand : class, ICommand =>
            Task.CompletedTask;
    }
}
