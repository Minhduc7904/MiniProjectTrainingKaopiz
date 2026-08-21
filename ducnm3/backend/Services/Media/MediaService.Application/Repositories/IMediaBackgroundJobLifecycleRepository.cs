// File: backend/Services/Media/MediaService.Application/Repositories/IMediaBackgroundJobLifecycleRepository.cs
// Mục đích: Định nghĩa port cập nhật lifecycle cho các Media background job được worker xử lý.

namespace MediaService.Application.Repositories;

public interface IMediaBackgroundJobLifecycleRepository
{
    Task StartAsync(
        Guid jobId,
        string jobType,
        string subjectType,
        Guid subjectId,
        Guid? correlationId,
        uint expectedItemCount,
        string payloadJson,
        CancellationToken cancellationToken);

    Task CompleteAsync(Guid jobId, uint processedItemCount, CancellationToken cancellationToken);

    Task FailAsync(Guid jobId, uint failedItemCount, string safeError, CancellationToken cancellationToken);
}
