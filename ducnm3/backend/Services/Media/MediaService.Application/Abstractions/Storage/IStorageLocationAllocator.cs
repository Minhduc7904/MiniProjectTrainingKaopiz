namespace MediaService.Application.Abstractions.Storage;

public interface IStorageLocationAllocator
{
    StorageObjectLocation Allocate(
        StorageMediaCategory category,
        string extension);
}
