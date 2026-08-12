namespace MediaService.Application.Storage;

public interface IStorageHealthProbe
{
    Task<StorageHealthProbeResult> CheckAsync(CancellationToken cancellationToken);
}
