// File: backend/Services/Media/MediaService.IntegrationTests/Storage/MinioStorageServiceTests.cs
// Mục đích: Kiểm thử adapter MinIO với storage thật, gồm lưu object, metadata, promote và các quy đổi lỗi storage.

using System.Text;
using MediaService.Application.Services.Storage;
using MediaService.Infrastructure.Storage.Minio;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Testcontainers.Minio;

namespace MediaService.IntegrationTests.Storage;

[TestFixture]
public sealed class MinioStorageServiceTests
{
    private readonly MinioContainer container = new MinioBuilder(
        "minio/minio:RELEASE.2025-09-07T16-13-09Z").Build();
    private IMinioClient client = null!;
    private MinioStorageService storage = null!;
    private MinioStorageLocationAllocator locationAllocator = null!;

    [OneTimeSetUp]
    public async Task StartMinioAsync()
    {
        await container.StartAsync();

        var endpoint = new Uri(container.GetConnectionString());
        var options = CreateOptions(endpoint.Authority);
        client = new MinioClient()
            .WithEndpoint(endpoint.Host, endpoint.Port)
            .WithCredentials(container.GetAccessKey(), container.GetSecretKey())
            .WithSSL(false)
            .Build();

        foreach (var bucket in options.GetBuckets())
        {
            await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket));
        }

        var objectKeyGenerator = new MinioObjectKeyGenerator(TimeProvider.System);
        locationAllocator = new MinioStorageLocationAllocator(
            Options.Create(options),
            objectKeyGenerator);
        storage = new MinioStorageService(
            new MinioInternalClient(client),
            Options.Create(options),
            NullLogger<MinioStorageService>.Instance);
    }

    [OneTimeTearDown]
    public async Task StopMinioAsync()
    {
        client?.Dispose();
        await container.DisposeAsync();
    }

    [TestCase(StorageMediaCategory.Image, "image/png", "png", "images")]
    [TestCase(StorageMediaCategory.Video, "video/mp4", "mp4", "videos")]
    [TestCase(StorageMediaCategory.Document, "application/pdf", "pdf", "documents")]
    [TestCase(StorageMediaCategory.Audio, "audio/mpeg", "mp3", "audios")]
    [TestCase(StorageMediaCategory.Other, "application/octet-stream", "bin", "other")]
    public async Task StorageLifecycleWorksForEachMediaCategory(
        StorageMediaCategory category,
        string contentType,
        string extension,
        string expectedBucket)
    {
        var bytes = Encoding.UTF8.GetBytes($"integration-{category}");
        await using var uploadStream = new MemoryStream(bytes);
        var location = locationAllocator.Allocate(category, extension);

        var uploaded = await storage.UploadAsync(
            new StorageUploadRequest(
                location,
                contentType,
                uploadStream,
                uploadStream.Length),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(uploaded.Bucket, Is.EqualTo(expectedBucket));
            Assert.That(uploaded.ObjectKey, Does.EndWith($".{extension}"));
            Assert.That(uploaded.Size, Is.EqualTo(bytes.Length));
            Assert.That(uploaded.ChecksumSha256, Has.Length.EqualTo(64));
        });
        Assert.That(
            await storage.ExistsAsync(location, CancellationToken.None),
            Is.True);

        var metadata = await storage.GetMetadataAsync(location, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(metadata.ContentType, Is.EqualTo(contentType));
            Assert.That(metadata.Size, Is.EqualTo(bytes.Length));
        });

        await using var downloadStream = new MemoryStream();
        var downloaded = await storage.DownloadAsync(
            new StorageDownloadRequest(location, downloadStream),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(downloaded.Size, Is.EqualTo(bytes.Length));
            Assert.That(downloadStream.ToArray(), Is.EqualTo(bytes));
        });

        await storage.DeleteAsync(location, CancellationToken.None);
        Assert.That(
            await storage.ExistsAsync(location, CancellationToken.None),
            Is.False);
    }

    [Test]
    public async Task HealthProbeIsHealthyWhenAllBucketsExist()
    {
        var result = await storage.CheckAsync(CancellationToken.None);

        Assert.That(result.IsHealthy, Is.True);
    }

    [Test]
    public async Task PromoteAsyncSourceChangesAfterHeadRejectsStaleEtag()
    {
        var source = locationAllocator.Allocate(StorageMediaCategory.Image, "png");
        var destination = locationAllocator.Allocate(StorageMediaCategory.Image, "png");
        await using (var initial = new MemoryStream(Encoding.UTF8.GetBytes("initial")))
        {
            await storage.UploadAsync(
                new StorageUploadRequest(source, "image/png", initial, initial.Length),
                CancellationToken.None);
        }

        var verified = await storage.GetMetadataAsync(source, CancellationToken.None);
        await using (var overwritten = new MemoryStream(Encoding.UTF8.GetBytes("overwritten")))
        {
            await storage.UploadAsync(
                new StorageUploadRequest(source, "image/png", overwritten, overwritten.Length),
                CancellationToken.None);
        }

        Assert.ThrowsAsync<StorageObjectNotFoundException>(() =>
            storage.PromoteAsync(
                new StoragePromotionRequest(source, destination, verified.ETag!),
                CancellationToken.None));
        Assert.That(
            await storage.ExistsAsync(destination, CancellationToken.None),
            Is.False);
    }

    private MinioStorageOptions CreateOptions(string endpoint) =>
        new()
        {
            Endpoint = endpoint,
            AccessKey = container.GetAccessKey(),
            SecretKey = container.GetSecretKey(),
            ImageBucket = "images",
            VideoBucket = "videos",
            DocumentBucket = "documents",
            AudioBucket = "audios",
            OtherBucket = "other"
        };
}
