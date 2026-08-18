// File: backend/Services/Media/MediaService.UnitTests/Storage/MinioUploadPolicyProviderTests.cs
// Mục đích: Kiểm thử ràng buộc và hành vi adapter MinIO tương ứng, gồm policy, object key, validation hoặc options.

using System.Text;
using MediaService.Application.Services.Storage;
using MediaService.Infrastructure.Storage.Minio;
using Microsoft.Extensions.Options;
using Minio;

namespace MediaService.UnitTests.Storage;

public sealed class MinioUploadPolicyProviderTests
{
    [Test]
    public async Task CreateAsyncUsesPublicEndpointAndExactSignedConstraints()
    {
        var options = new MinioStorageOptions
        {
            Endpoint = "minio:9000",
            PublicEndpoint = "uploads.example.test:9443",
            PublicUseSsl = true,
            AccessKey = "test-access",
            SecretKey = "test-secret-key",
            ImageBucket = "images",
            VideoBucket = "videos",
            DocumentBucket = "documents",
            AudioBucket = "audios",
            OtherBucket = "others",
            UploadPresignExpirySeconds = 900,
        };
        using var client = new MinioClient()
            .WithEndpoint(options.PublicEndpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .WithSSL(options.PublicUseSsl)
            .Build();
        var provider = new MinioUploadPolicyProvider(
            new MinioSigningClient(client), Options.Create(options), TimeProvider.System);

        var result = await provider.CreateAsync(
            new StorageUploadPolicyRequest(
                new StorageObjectLocation("images", "2026/08/exact.png"),
                "image/png", 123, new string('a', 64)),
            TestContext.CurrentContext.CancellationToken);
        var policyJson = Encoding.UTF8.GetString(
            Convert.FromBase64String(result.FormFields["policy"]));

        Assert.Multiple(() =>
        {
            Assert.That(result.UploadUrl.Scheme, Is.EqualTo("https"));
            Assert.That(result.UploadUrl.Host, Is.EqualTo("uploads.example.test"));
            Assert.That(result.FormFields["Content-Type"], Is.EqualTo("image/png"));
            Assert.That(policyJson, Does.Contain("2026/08/exact.png"));
            Assert.That(policyJson, Does.Contain("image/png"));
            Assert.That(policyJson, Does.Contain("content-length-range"));
            Assert.That(policyJson, Does.Contain("\"123\",\"123\""));
            Assert.That(policyJson, Does.Contain("x-amz-meta-checksum-sha256"));
            Assert.That(policyJson, Does.Contain(new string('a', 64)));
        });
    }
}
