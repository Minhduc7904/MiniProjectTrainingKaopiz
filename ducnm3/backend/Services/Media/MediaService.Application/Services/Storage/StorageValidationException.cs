// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageValidationException.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed class StorageValidationException(string message) : Exception(message);
