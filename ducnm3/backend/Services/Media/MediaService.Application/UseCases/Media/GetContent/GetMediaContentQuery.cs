// File: backend/Services/Media/MediaService.Application/UseCases/Media/GetContent/GetMediaContentQuery.cs
// Mục đích: Định nghĩa dữ liệu truy vấn cho use case GetMediaContentQuery.

namespace MediaService.Application.UseCases.Media.GetContent;

public sealed record GetMediaContentQuery(Guid MediaId);
