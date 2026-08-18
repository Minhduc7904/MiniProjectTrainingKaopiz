// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/Dispatch/NotificationBatchProcessingOptions.cs
// Mục đích: Định nghĩa options cấu hình được bind từ application configuration cho NotificationBatchProcessingOptions.

namespace NotificationService.Application.Features.Batches;

public sealed class NotificationBatchProcessingOptions
{
    public const string SectionName = "NotificationBatchProcessing";

    public int DispatchChunkConcurrency { get; init; } = 1;

    public int MaxConcurrentSends { get; init; } = 1;

    public int ClaimLeaseSeconds { get; init; } = 120;

    public void Validate()
    {
        if (DispatchChunkConcurrency is < 1 or > 32)
        {
            throw new InvalidOperationException(
                $"{SectionName}:DispatchChunkConcurrency must be between 1 and 32.");
        }

        if (MaxConcurrentSends is < 1 or > 256)
        {
            throw new InvalidOperationException(
                $"{SectionName}:MaxConcurrentSends must be between 1 and 256.");
        }

        if (ClaimLeaseSeconds is < 30 or > 3_600)
        {
            throw new InvalidOperationException(
                $"{SectionName}:ClaimLeaseSeconds must be between 30 and 3600.");
        }
    }
}
