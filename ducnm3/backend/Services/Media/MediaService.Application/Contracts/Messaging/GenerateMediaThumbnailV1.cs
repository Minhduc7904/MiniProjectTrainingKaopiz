using BuildingBlocks.Messaging.Abstractions;

namespace MediaService.Application.Contracts.Messaging;

public sealed record GenerateMediaThumbnailV1(
    Guid JobId,
    Guid SourceMediaId,
    Guid DerivativeMediaId) : ICommand;
