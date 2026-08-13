namespace MediaService.Application.Abstractions.Storage;

public sealed class StorageOperationException : Exception
{
    public StorageOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
