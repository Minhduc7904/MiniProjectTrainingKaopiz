// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/Create/CreateMediaUsageCommand.cs
// Mục đích: Định nghĩa dữ liệu đầu vào cho use case CreateMediaUsageCommand.

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
