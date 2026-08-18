// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/Create/CreateMediaUsageResult.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

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
