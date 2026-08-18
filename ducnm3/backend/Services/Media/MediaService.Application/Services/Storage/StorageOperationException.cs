// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageOperationException.cs
// Mục đích: Biểu diễn lỗi vận hành storage như MinIO không sẵn sàng để API có thể trả trạng thái phù hợp.

namespace MediaService.Application.Services.Storage;

public sealed class StorageOperationException : Exception
{
    public StorageOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
