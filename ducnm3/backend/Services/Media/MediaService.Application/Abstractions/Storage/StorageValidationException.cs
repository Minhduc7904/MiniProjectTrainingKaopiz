namespace MediaService.Application.Abstractions.Storage;

public sealed class StorageValidationException(string message) : Exception(message);
