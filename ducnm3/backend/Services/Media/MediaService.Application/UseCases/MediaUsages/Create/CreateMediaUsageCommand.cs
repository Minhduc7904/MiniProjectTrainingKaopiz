// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/Create/CreateMediaUsageCommand.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.MediaUsages.Create;

public sealed record CreateMediaUsageCommand(
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    ActorReference CreatedBy);
