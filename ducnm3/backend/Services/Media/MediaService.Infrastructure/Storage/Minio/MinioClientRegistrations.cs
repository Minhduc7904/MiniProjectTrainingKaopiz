// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/MinioClientRegistrations.cs
// Mục đích: Đăng ký MinIO clients vào DI, tách client thao tác nội bộ với client tạo presigned upload policy.

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
