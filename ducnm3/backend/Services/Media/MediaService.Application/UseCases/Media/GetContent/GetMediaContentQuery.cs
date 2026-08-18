// File: backend/Services/Media/MediaService.Application/UseCases/Media/GetContent/GetMediaContentQuery.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.UseCases.Media.GetContent;

public sealed record GetMediaContentQuery(Guid MediaId);
