using MediaService.Application.Storage;
using MediaService.Infrastructure.Storage.Minio;

namespace MediaService.UnitTests.Storage;

public class MinioStorageRequestValidatorTests
{
    [TestCase(StorageMediaCategory.Image, "image/png")]
    [TestCase(StorageMediaCategory.Video, "video/mp4")]
    [TestCase(StorageMediaCategory.Document, "application/pdf")]
    [TestCase(StorageMediaCategory.Audio, "audio/mpeg")]
    [TestCase(StorageMediaCategory.Other, "application/octet-stream")]
    public void ValidateUploadAcceptsMatchingCategoryAndContentType(
        StorageMediaCategory category,
        string contentType)
    {
        using var content = new MemoryStream([1, 2, 3]);
        var request = new StorageUploadRequest(category, contentType, ".BIN", content, content.Length);

        var result = MinioStorageRequestValidator.ValidateUpload(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.ContentType, Is.EqualTo(contentType));
            Assert.That(result.Extension, Is.EqualTo("bin"));
        });
    }

    [Test]
    public void ValidateUploadRejectsMismatchedCategory()
    {
        using var content = new MemoryStream([1]);
        var request = new StorageUploadRequest(
            StorageMediaCategory.Video,
            "image/png",
            "png",
            content,
            content.Length);

        Assert.Throws<StorageValidationException>(
            () => MinioStorageRequestValidator.ValidateUpload(request));
    }

    [Test]
    public void ValidateUploadRejectsUnsafeExtension()
    {
        using var content = new MemoryStream([1]);
        var request = new StorageUploadRequest(
            StorageMediaCategory.Image,
            "image/png",
            "../png",
            content,
            content.Length);

        Assert.Throws<StorageValidationException>(
            () => MinioStorageRequestValidator.ValidateUpload(request));
    }

    [Test]
    public void ValidateUploadRejectsSizeMismatch()
    {
        using var content = new MemoryStream([1, 2]);
        var request = new StorageUploadRequest(
            StorageMediaCategory.Image,
            "image/png",
            "png",
            content,
            1);

        Assert.Throws<StorageValidationException>(
            () => MinioStorageRequestValidator.ValidateUpload(request));
    }
}
