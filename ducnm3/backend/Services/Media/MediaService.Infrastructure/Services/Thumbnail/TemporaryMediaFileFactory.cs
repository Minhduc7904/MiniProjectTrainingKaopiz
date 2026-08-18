// File: backend/Services/Media/MediaService.Infrastructure/Services/Thumbnail/TemporaryMediaFileFactory.cs
// Mục đích: Tạo và dọn file tạm dùng khi Media Service tải source về để sinh thumbnail, tránh để lại file hệ thống.

using MediaService.Application.Services.Derivation;

namespace MediaService.Infrastructure.Services.Thumbnail;

public sealed class TemporaryMediaFileFactory : ITemporaryMediaFileFactory
{
    public Task<ITemporaryMediaFile> CreateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "media-thumbnail");
        Directory.CreateDirectory(directory);
        var path = System.IO.Path.Combine(directory, $"{Guid.NewGuid():N}.source");
        ITemporaryMediaFile file = new TemporaryMediaFile(path);
        return Task.FromResult(file);
    }

    private sealed class TemporaryMediaFile : ITemporaryMediaFile
    {
        public TemporaryMediaFile(string path)
        {
            Path = path;
            Stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.Read,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

        public string Path { get; }

        public Stream Stream { get; }

        public async ValueTask DisposeAsync()
        {
            await Stream.DisposeAsync();
            try
            {
                File.Delete(Path);
            }
            catch (FileNotFoundException)
            {
            }
        }
    }
}
