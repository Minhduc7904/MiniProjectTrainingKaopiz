// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageObjectNotFoundException.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed class StorageObjectNotFoundException(string message) : Exception(message);
