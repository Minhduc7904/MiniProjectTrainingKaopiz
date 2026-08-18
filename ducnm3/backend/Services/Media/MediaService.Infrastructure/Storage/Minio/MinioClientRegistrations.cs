// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/MinioClientRegistrations.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using Minio;

namespace MediaService.Infrastructure.Storage.Minio;

public sealed class MinioInternalClient(IMinioClient client) : IDisposable
{
    public IMinioClient Client { get; } = client;

    public void Dispose() => Client.Dispose();
}

public sealed class MinioSigningClient(IMinioClient client) : IDisposable
{
    public IMinioClient Client { get; } = client;

    public void Dispose() => Client.Dispose();
}
