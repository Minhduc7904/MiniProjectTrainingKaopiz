// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/Complete/CompleteDirectUploadCommand.cs
// Mục đích: Định nghĩa dữ liệu đầu vào cho use case CompleteDirectUploadCommand.

using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.Media.DirectUpload.Complete;

public sealed record CompleteDirectUploadCommand(
    Guid MediaId,
    ActorReference UploadedBy);
