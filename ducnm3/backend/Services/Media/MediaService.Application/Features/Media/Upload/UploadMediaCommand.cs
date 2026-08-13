using MediaService.Domain.Actors;

namespace MediaService.Application.Features.Media.Upload;

public sealed record UploadMediaCommand(
    string MediaType,
    string ContentType,
    string OriginalFileName,
    Stream Content,
    long SizeBytes,
    ActorReference UploadedBy);
