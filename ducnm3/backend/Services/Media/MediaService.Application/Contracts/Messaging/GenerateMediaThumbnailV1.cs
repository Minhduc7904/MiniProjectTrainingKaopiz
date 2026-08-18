// File: backend/Services/Media/MediaService.Application/Contracts/Messaging/GenerateMediaThumbnailV1.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

using BuildingBlocks.Messaging.Abstractions;

namespace MediaService.Application.Contracts.Messaging;

public sealed record GenerateMediaThumbnailV1(
    Guid JobId,
    Guid SourceMediaId,
    Guid DerivativeMediaId) : ICommand;
