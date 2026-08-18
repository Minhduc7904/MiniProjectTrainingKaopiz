// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageOperationException.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed class StorageOperationException : Exception
{
    public StorageOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
