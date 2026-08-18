// File: backend/Services/Media/MediaService.UnitTests/Storage/MinioStorageRequestValidatorTests.cs
// Mục đích: Kiểm thử ràng buộc và hành vi adapter MinIO tương ứng, gồm policy, object key, validation hoặc options.

using MediaService.Application.Services.Storage;
using MediaService.Application.UseCases.Media.DirectUpload;
using MediaService.Infrastructure.Storage.Minio;

namespace MediaService.UnitTests.Storage;

public class MinioStorageRequestValidatorTests
{
    [TestCase("image/png")]
    [TestCase("video/mp4")]
    [TestCase("application/pdf")]
    public void ValidateUploadAcceptsReadableContent(string contentType)
    {
        using var content = new MemoryStream([1, 2, 3]);
        var request = new StorageUploadRequest(
            new StorageObjectLocation("images", "2026/08/13/file.png"),
            contentType,
            content,
            content.Length);

        var result = MinioStorageRequestValidator.ValidateUpload(request);

        Assert.That(result.ContentType, Is.EqualTo(contentType));
    }

    [Test]
    public void ValidateUploadRejectsMismatchedCategory()
    {
        Assert.That(
            MediaContentTypeRules.Matches(
                StorageMediaCategory.Video,
                "image/png"),
            Is.False);
    }

    [Test]
    public void ValidateUploadRejectsUnsafeExtension()
    {
        Assert.Throws<StorageValidationException>(
            () => MinioStorageRequestValidator.NormalizeExtension("../png"));
    }

    [Test]
    public void ValidateUploadRejectsSizeMismatch()
    {
        using var content = new MemoryStream([1, 2]);
        var request = new StorageUploadRequest(
            new StorageObjectLocation("images", "2026/08/13/file.png"),
            "image/png",
            content,
            1);

        Assert.Throws<StorageValidationException>(
            () => MinioStorageRequestValidator.ValidateUpload(request));
    }
}
