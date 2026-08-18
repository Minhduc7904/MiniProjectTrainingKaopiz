// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/Complete/CompleteDirectUploadCommand.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.Media.DirectUpload.Complete;

public sealed record CompleteDirectUploadCommand(
    Guid MediaId,
    ActorReference UploadedBy);
