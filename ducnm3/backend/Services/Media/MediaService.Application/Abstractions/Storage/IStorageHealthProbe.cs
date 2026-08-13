namespace MediaService.Application.Abstractions.Storage;

public interface IStorageHealthProbe
{
    Task<StorageHealthProbeResult> CheckAsync(CancellationToken cancellationToken);
}
