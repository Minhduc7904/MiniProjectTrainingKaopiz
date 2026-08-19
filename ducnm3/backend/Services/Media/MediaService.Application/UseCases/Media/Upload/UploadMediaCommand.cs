// File: backend/Services/Media/MediaService.Application/UseCases/Media/Upload/UploadMediaCommand.cs
// Mục đích: Định nghĩa dữ liệu đầu vào cho use case UploadMediaCommand.

using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.Media.Upload;

public sealed record UploadMediaCommand(
    string MediaType,
    string ContentType,
    string OriginalFileName,
    Stream Content,
    long SizeBytes,
    ActorReference UploadedBy);
