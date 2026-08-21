// File: backend/Services/Media/MediaService.Infrastructure/Persistence/MediaBackgroundJobPayload.cs
// Mục đích: Định dạng payload JSON versioned của job chung, phục vụ retry không phụ thuộc transport message cũ.

namespace MediaService.Infrastructure.Persistence;

internal sealed record MediaBackgroundJobPayload(
    int Version,
    Guid? SourceMediaId = null,
    Guid? DerivativeMediaId = null,
    IReadOnlyList<string>? FailedChunks = null);
