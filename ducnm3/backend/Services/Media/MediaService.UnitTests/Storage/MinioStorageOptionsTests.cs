using MediaService.Application.Abstractions.Storage;
using MediaService.Infrastructure.Storage.Minio;

namespace MediaService.UnitTests.Storage;

public class MinioStorageOptionsTests
{
    [Test]
    public void ValidatorAcceptsCompleteConfiguration()
    {
        var result = new MinioStorageOptionsValidator().Validate(null, CreateValidOptions());

        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void ValidatorRejectsDuplicateOrMalformedBuckets()
    {
        var options = new MinioStorageOptions
        {
            Endpoint = "minio:9000",
            AccessKey = "media-app",
            SecretKey = "long-media-secret",
            ImageBucket = "images",
            VideoBucket = "images",
            DocumentBucket = "Invalid_Bucket",
            AudioBucket = "audios",
            OtherBucket = "other"
        };

        var result = new MinioStorageOptionsValidator().Validate(null, options);

        Assert.That(result.Failed, Is.True);
        Assert.That(result.Failures, Has.Some.Contains("distinct"));
        Assert.That(result.Failures, Has.Some.Contains("Invalid_Bucket"));
    }

    [TestCase(StorageMediaCategory.Image, "images")]
    [TestCase(StorageMediaCategory.Video, "videos")]
    [TestCase(StorageMediaCategory.Document, "documents")]
    [TestCase(StorageMediaCategory.Audio, "audios")]
    [TestCase(StorageMediaCategory.Other, "other")]
    public void CategoryMapsToConfiguredBucket(
        StorageMediaCategory category,
        string expectedBucket)
    {
        var options = CreateValidOptions();

        Assert.That(options.GetBucket(category), Is.EqualTo(expectedBucket));
    }

    private static MinioStorageOptions CreateValidOptions() =>
        new()
        {
            Endpoint = "minio:9000",
            AccessKey = "media-app",
            SecretKey = "long-media-secret",
            ImageBucket = "images",
            VideoBucket = "videos",
            DocumentBucket = "documents",
            AudioBucket = "audios",
            OtherBucket = "other"
        };
}
