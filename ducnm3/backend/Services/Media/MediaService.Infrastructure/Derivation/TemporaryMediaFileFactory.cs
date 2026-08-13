using MediaService.Application.Abstractions.Derivation;

namespace MediaService.Infrastructure.Derivation;

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
