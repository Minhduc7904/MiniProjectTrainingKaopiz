// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/MinioObjectKeyGenerator.cs
// Mục đích: Sinh object key MinIO có namespace theo Media ID và category để tách file, tránh trùng đường dẫn và dễ dọn rác.

using System.Globalization;

namespace MediaService.Infrastructure.Storage.Minio;

public sealed class MinioObjectKeyGenerator(TimeProvider timeProvider)
{
    public string Create(string normalizedExtension)
    {
        var utcNow = timeProvider.GetUtcNow();
        var datePrefix = utcNow.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        return $"{datePrefix}/{Guid.NewGuid():N}.{normalizedExtension}";
    }
}
