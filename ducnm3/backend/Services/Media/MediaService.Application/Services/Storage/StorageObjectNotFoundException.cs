// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageObjectNotFoundException.cs
// Mục đích: Biểu diễn lỗi object không tồn tại trong storage để use case phân biệt với lỗi hạ tầng tạm thời.

namespace MediaService.Application.Services.Storage;

public sealed class StorageObjectNotFoundException(string message) : Exception(message);
