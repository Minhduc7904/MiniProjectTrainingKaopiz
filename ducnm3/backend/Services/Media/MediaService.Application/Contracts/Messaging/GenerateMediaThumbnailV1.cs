// File: backend/Services/Media/MediaService.Application/Contracts/Messaging/GenerateMediaThumbnailV1.cs
// Mục đích: Khai báo hợp đồng message GenerateMediaThumbnailV1 dùng để giao tiếp bất đồng bộ giữa các service.

using BuildingBlocks.Messaging.Abstractions;

namespace MediaService.Application.Contracts.Messaging;

public sealed record GenerateMediaThumbnailV1(
    Guid JobId,
    Guid SourceMediaId,
    Guid DerivativeMediaId) : ICommand;
