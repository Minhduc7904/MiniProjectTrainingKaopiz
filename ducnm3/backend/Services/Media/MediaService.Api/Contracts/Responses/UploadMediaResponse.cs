namespace MediaService.Api.Contracts.Responses;

public sealed record UploadMediaResponse(
    Guid Id,
    string MediaType,
    string ContentType,
    long SizeBytes,
    string Status,
    string ContentUrl);
