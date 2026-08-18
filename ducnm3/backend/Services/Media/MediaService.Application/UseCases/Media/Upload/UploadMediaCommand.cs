// File: backend/Services/Media/MediaService.Application/UseCases/Media/Upload/UploadMediaCommand.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.Media.Upload;

public sealed record UploadMediaCommand(
    string MediaType,
    string ContentType,
    string OriginalFileName,
    Stream Content,
    long SizeBytes,
    ActorReference UploadedBy);
