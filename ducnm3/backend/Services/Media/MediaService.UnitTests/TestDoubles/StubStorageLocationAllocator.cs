// File: backend/Services/Media/MediaService.UnitTests/TestDoubles/StubStorageLocationAllocator.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Services.Storage;

namespace MediaService.UnitTests.TestDoubles;

public sealed class StubStorageLocationAllocator : IStorageLocationAllocator
{
    private int sequence;

    public StorageObjectLocation Allocate(
        StorageMediaCategory category,
        string extension) =>
        new(category.ToString().ToLowerInvariant() + "s", $"test/{++sequence}{extension}");
}
