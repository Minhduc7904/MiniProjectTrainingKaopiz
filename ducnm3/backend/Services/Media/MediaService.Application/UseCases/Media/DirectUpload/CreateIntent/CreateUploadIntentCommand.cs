// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/CreateIntent/CreateUploadIntentCommand.cs
// Mục đích: Định nghĩa dữ liệu đầu vào cho use case CreateUploadIntentCommand.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.Media.DirectUpload.CreateIntent;

public sealed record CreateUploadIntentCommand(
    string OriginalFileName,
    string MediaType,
    string ContentType,
    long SizeBytes,
    string ChecksumSha256,
    ActorReference UploadedBy);
