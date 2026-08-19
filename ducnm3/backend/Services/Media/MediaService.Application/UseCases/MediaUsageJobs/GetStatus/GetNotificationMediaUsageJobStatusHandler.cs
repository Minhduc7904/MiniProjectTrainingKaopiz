// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsageJobs/GetStatus/GetNotificationMediaUsageJobStatusHandler.cs
// Mục đích: Đọc job Media Usage, tính remaining/progress và trả lỗi 404 an toàn khi job không tồn tại.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;

namespace MediaService.Application.UseCases.MediaUsageJobs.GetStatus;

public sealed class GetNotificationMediaUsageJobStatusHandler(
    INotificationMediaUsageJobRepository repository)
{
    public async Task<NotificationMediaUsageJobStatusResult> HandleAsync(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        if (jobId == Guid.Empty)
        {
            throw MediaErrors.InvalidMedia("jobId must be a valid UUID.");
        }

        var job = await repository.GetByIdAsync(jobId, cancellationToken)
            ?? throw MediaErrors.NotificationMediaUsageJobNotFound();
        var handled = checked(job.ProcessedUsageCount + job.FailedUsageCount);
        uint? remaining = job.ExpectedUsageCount is null
            ? null
            : job.ExpectedUsageCount.Value > handled
                ? job.ExpectedUsageCount.Value - handled
                : 0;
        decimal? percent = job.ExpectedUsageCount is > 0
            ? Math.Min(100m, Math.Round(handled * 100m / job.ExpectedUsageCount.Value, 2))
            : job.ExpectedUsageCount == 0 ? 100m : null;
        return new NotificationMediaUsageJobStatusResult(
            job.Id, job.Status, job.ExpectedUsageCount, job.ProcessedUsageCount,
            job.FailedUsageCount, remaining, percent, job.CreatedAtUtc,
            job.StartedAtUtc, job.CompletedAtUtc, job.LastError);
    }
}
