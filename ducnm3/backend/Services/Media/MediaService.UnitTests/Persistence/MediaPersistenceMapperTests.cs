// File: backend/Services/Media/MediaService.UnitTests/Persistence/MediaPersistenceMapperTests.cs
// Mục đích: Kiểm thử mapper chuyển entity EF Media sang dữ liệu Application mà không mất các trường lifecycle và ownership.

using MediaService.Domain.Entities;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DatabaseMediaObject = MediaService.Infrastructure.Persistence.Scaffolded.MediaObject;
using DatabaseMediaUsage = MediaService.Infrastructure.Persistence.Scaffolded.MediaUsage;

namespace MediaService.UnitTests.Persistence;

public sealed class MediaPersistenceMapperTests
{
    [Test]
    public void MediaObjectIsDraftDatabaseDefaultIsTrueUsesTrueAsSentinel()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseMySql(
                "Server=localhost;Database=media_test;User=root;Password=not-used;",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        using var dbContext = new MediaDbContext(options);

        // Act
        var property = dbContext.Model
            .FindEntityType(typeof(DatabaseMediaObject))!
            .FindProperty(nameof(DatabaseMediaObject.IsDraft))!;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(property.GetDefaultValueSql(), Is.EqualTo("'1'"));
            Assert.That(property.Sentinel, Is.True);
        });
    }

    [Test]
    public void ToDomainScaffoldedMediaObjectMapsMediaEntity()
    {
        // Arrange
        var uploadedAtUtc = new DateTime(2026, 8, 18, 8, 0, 0, DateTimeKind.Utc);
        var draftedAtUtc = uploadedAtUtc.AddMinutes(1);
        var databaseModel = new DatabaseMediaObject
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Bucket = "images",
            ObjectKey = "2026/08/example.png",
            MediaType = "IMAGE",
            ContentType = "image/png",
            OriginalFileName = "example.png",
            SizeBytes = 123,
            Status = "READY",
            CreatedAt = uploadedAtUtc,
            IsDraft = true,
            DraftedAt = draftedAtUtc,
            ChecksumSha256 = new string('a', 64),
            UploadedBy = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            UploadedByType = "STUDENT",
        };

        // Act
        Media media = MediaPersistenceMapper.ToDomain(databaseModel);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(media.Id, Is.EqualTo(databaseModel.Id));
            Assert.That(media.Bucket, Is.EqualTo("images"));
            Assert.That(media.ObjectKey, Is.EqualTo("2026/08/example.png"));
            Assert.That(media.SizeBytes, Is.EqualTo(123));
            Assert.That(media.IsDraft, Is.True);
            Assert.That(media.DraftedAtUtc, Is.EqualTo(draftedAtUtc));
            Assert.That(media.UploadedBy!.Id, Is.EqualTo(databaseModel.UploadedBy));
            Assert.That(media.UploadedBy.Type, Is.EqualTo("STUDENT"));
        });
    }

    [Test]
    public void ToDomainScaffoldedMediaUsageMapsMediaUsageEntity()
    {
        // Arrange
        var createdAtUtc = new DateTime(2026, 8, 18, 8, 0, 0, DateTimeKind.Utc);
        var databaseModel = new DatabaseMediaUsage
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            MediaId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            OwnerService = "STUDENT",
            OwnerType = "STUDENT_AVATAR",
            OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            UsageType = "AVATAR",
            DisplayOrder = 0,
            CreatedBy = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CreatedByType = "STUDENT",
            CreatedAt = createdAtUtc,
        };

        // Act
        MediaUsage usage = MediaUsagePersistenceMapper.ToDomain(databaseModel);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(usage.Id, Is.EqualTo(databaseModel.Id));
            Assert.That(usage.MediaId, Is.EqualTo(databaseModel.MediaId));
            Assert.That(usage.OwnerId, Is.EqualTo(databaseModel.OwnerId));
            Assert.That(usage.CreatedBy.Id, Is.EqualTo(databaseModel.CreatedBy));
            Assert.That(usage.CreatedAtUtc, Is.EqualTo(createdAtUtc));
        });
    }
}
