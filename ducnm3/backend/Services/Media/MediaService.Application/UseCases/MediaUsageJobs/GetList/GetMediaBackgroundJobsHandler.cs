// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsageJobs/GetList/GetMediaBackgroundJobsHandler.cs
// Mục đích: Validate quyền ADMIN, filter và pagination trước khi đọc Media background job.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Actors;
using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.MediaUsageJobs.GetList;

public sealed class GetMediaBackgroundJobsHandler(
    IMediaBackgroundJobListRepository repository,
    IActorValidationService actorValidationService)
{
    public async Task<GetMediaBackgroundJobsResult> HandleAsync(
        GetMediaBackgroundJobsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.Page < 1) throw MediaErrors.InvalidMediaJobQuery("page must be at least 1.");
        if (query.PageSize is < 1 or > 100) throw MediaErrors.InvalidMediaJobQuery("pageSize must be between 1 and 100.");

        var actor = await actorValidationService.ValidateAsync(query.Actor, cancellationToken);
        if (actor.Type != ActorTypes.Admin)
            throw MediaErrors.InvalidActorType("Only an ADMIN actor can list media jobs.");

        var jobType = NormalizeAllowlisted(query.JobType, MediaBackgroundJobTypes.All, "jobType");
        var status = NormalizeAllowlisted(query.Status, MediaBackgroundJobStatuses.All, "status");
        var correlationId = ParseOptionalGuid(query.CorrelationId, "correlationId");
        var page = await repository.ListAsync(
            new MediaBackgroundJobListRequest(jobType, status, correlationId, query.Page, query.PageSize),
            cancellationToken);
        var totalPages = page.TotalItems == 0 ? 0 : checked((int)Math.Ceiling(page.TotalItems / (double)query.PageSize));
        return new GetMediaBackgroundJobsResult(page.Items, query.Page, query.PageSize, page.TotalItems, totalPages);
    }

    private static string? NormalizeAllowlisted(string? value, IReadOnlySet<string> allowed, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToUpperInvariant();
        return allowed.Contains(normalized)
            ? normalized
            : throw MediaErrors.InvalidMediaJobQuery($"{field} is not supported.");
    }

    private static Guid? ParseOptionalGuid(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Guid.TryParse(value, out var parsed) && parsed != Guid.Empty
            ? parsed
            : throw MediaErrors.InvalidMediaJobQuery($"{field} must be a valid UUID.");
    }
}
