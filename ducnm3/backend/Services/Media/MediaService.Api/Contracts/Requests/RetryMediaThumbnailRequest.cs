namespace MediaService.Api.Contracts.Requests;

public sealed record RetryMediaThumbnailRequest(
    string RequestedBy,
    string RequestedByType);
