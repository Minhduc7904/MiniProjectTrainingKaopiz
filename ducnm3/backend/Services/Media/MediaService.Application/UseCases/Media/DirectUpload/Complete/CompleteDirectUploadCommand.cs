// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/Complete/CompleteDirectUploadCommand.cs
// Mục đích: Định nghĩa dữ liệu đầu vào cho use case CompleteDirectUploadCommand.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.UseCases.Media.DirectUpload.Complete;

public sealed record CompleteDirectUploadCommand(
    Guid MediaId,
    ActorReference UploadedBy);
