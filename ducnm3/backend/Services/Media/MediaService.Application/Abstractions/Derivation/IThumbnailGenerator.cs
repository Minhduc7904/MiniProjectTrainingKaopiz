namespace MediaService.Application.Abstractions.Derivation;

public interface IThumbnailGenerator
{
    Task<GeneratedThumbnail> GenerateAsync(
        ThumbnailGenerationRequest request,
        CancellationToken cancellationToken);
}

public sealed record ThumbnailGenerationRequest(
    string SourcePath,
    string MediaType,
    string ContentType);

public sealed record GeneratedThumbnail(
    byte[] Content,
    int Width,
    int Height);

public interface ITemporaryMediaFileFactory
{
    Task<ITemporaryMediaFile> CreateAsync(CancellationToken cancellationToken);
}

public interface ITemporaryMediaFile : IAsyncDisposable
{
    string Path { get; }

    Stream Stream { get; }
}
