// File: backend/Services/Media/MediaService.Application/Services/Derivation/IThumbnailGenerator.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Derivation;

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
