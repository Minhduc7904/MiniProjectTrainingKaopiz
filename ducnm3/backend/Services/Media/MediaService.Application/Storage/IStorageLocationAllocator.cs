namespace MediaService.Application.Storage;

public interface IStorageLocationAllocator
{
    StorageObjectLocation Allocate(
        StorageMediaCategory category,
        string extension);
}
