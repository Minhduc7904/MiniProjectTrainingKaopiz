namespace MediaService.Application.Storage;

public sealed class StorageValidationException(string message) : Exception(message);

public sealed class StorageOperationException : Exception
{
    public StorageOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
