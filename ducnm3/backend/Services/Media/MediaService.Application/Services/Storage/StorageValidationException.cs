// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageValidationException.cs
// Mục đích: Biểu diễn lỗi metadata hoặc nội dung object không đáp ứng ràng buộc upload của storage.

namespace MediaService.Application.Services.Storage;

public sealed class StorageValidationException(string message) : Exception(message);
