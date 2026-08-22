// File: backend/Services/Media/MediaService.Application/UseCases/Media/GetSummary/GetMediaSummaryHandler.cs
// Mục đích: Điều phối use case đọc tổng số media object mà không phụ thuộc EF Core.

using MediaService.Application.Repositories;

namespace MediaService.Application.UseCases.Media.GetSummary;

public sealed class GetMediaSummaryHandler(IMediaSummaryRepository repository)
{
    public async Task<MediaSummaryResult> HandleAsync(CancellationToken cancellationToken) =>
        new(await repository.CountAsync(cancellationToken));
}

public sealed record MediaSummaryResult(long TotalMedia);
