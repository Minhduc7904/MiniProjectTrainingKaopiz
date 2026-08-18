// File: backend/Services/Media/MediaService.Application/Services/Urls/MediaUrl.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Urls;

public sealed record MediaUrl(string Value, DateTime? ExpiresAtUtc);
