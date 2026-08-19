// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsageJobs/Process/NotificationMediaUsageJobLifecycleHandler.cs
// Mục đích: Xử lý các command mở, đóng và ghi lỗi job Media Usage mà Worker nhận từ RabbitMQ.

using MediaService.Application.Repositories;
using MediaService.Contracts.Messaging;
using MediaService.Application.Common.Errors;

namespace MediaService.Application.UseCases.MediaUsageJobs.Process;

public sealed class NotificationMediaUsageJobLifecycleHandler(
    INotificationMediaUsageJobRepository repository)
{
    public Task HandleAsync(StartNotificationMediaUsageJobV1 command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateJobId(command.JobId);
        return repository.StartAsync(command.JobId, cancellationToken);
    }

    public Task HandleAsync(CompleteNotificationMediaUsageJobV1 command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateJobId(command.JobId);
        return repository.CompleteSourceAsync(command.JobId, command.ExpectedUsageCount, cancellationToken);
    }

    public Task RecordFailureAsync(
        RegisterNotificationMediaUsageBatchV1 command,
        string safeError,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateJobId(command.JobId);
        var usageCount = checked((uint)(command.NotificationIds.Count * command.References.Count));
        return repository.RecordFailureAsync(command.JobId, usageCount, safeError, cancellationToken);
    }

    private static void ValidateJobId(Guid jobId)
    {
        if (jobId == Guid.Empty)
        {
            throw MediaErrors.InvalidMedia("Notification media usage jobId is invalid.");
        }
    }
}
