// File: backend/Services/Media/MediaService.Application/Services/Urls/MediaUrl.cs
// Mục đích: Đóng gói URL media đã được tạo để use case trả về dữ liệu truy cập nội dung một cách rõ ràng.

namespace MediaService.Application.Services.Urls;

public sealed record MediaUrl(string Value, DateTime? ExpiresAtUtc);
