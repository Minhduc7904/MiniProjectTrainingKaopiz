// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/Create/CreateMediaUsageResult.cs
// Mục đích: Định nghĩa dữ liệu đầu ra của use case CreateMediaUsageResult.

namespace MediaService.Application.UseCases.MediaUsages.Create;

public sealed record CreateMediaUsageResult(
    Guid Id,
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    DateTime CreatedAtUtc);
