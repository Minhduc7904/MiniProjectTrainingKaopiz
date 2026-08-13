namespace MediaService.Application.Features.Media.Upload;

public sealed record UploadMediaResult(
    Guid Id,
    string MediaType,
    string ContentType,
    long SizeBytes,
    string Status);
