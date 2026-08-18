// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/MinioObjectKeyGenerator.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

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
