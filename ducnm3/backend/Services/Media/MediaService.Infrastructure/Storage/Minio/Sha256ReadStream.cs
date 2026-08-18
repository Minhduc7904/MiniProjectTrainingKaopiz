// File: backend/Services/Media/MediaService.Infrastructure/Storage/Minio/Sha256ReadStream.cs
// Mục đích: Bọc stream đọc để tính SHA-256 khi truyền file qua storage, phục vụ xác minh tính toàn vẹn file.

using System.Security.Cryptography;

namespace MediaService.Infrastructure.Storage.Minio;

internal sealed class Sha256ReadStream(Stream inner) : Stream
{
    private readonly IncrementalHash hash =
        IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    private bool hashFinalized;

    public override bool CanRead => inner.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => inner.Length;

    public override long Position
    {
        get => inner.Position;
        set => throw new NotSupportedException();
    }

    public string GetChecksumHex()
    {
        ObjectDisposedException.ThrowIf(hashFinalized, this);
        hashFinalized = true;
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = inner.Read(buffer, offset, count);
        Append(buffer.AsSpan(offset, read));
        return read;
    }

    public override int Read(Span<byte> buffer)
    {
        var read = inner.Read(buffer);
        Append(buffer[..read]);
        return read;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        var read = await inner.ReadAsync(buffer, cancellationToken);
        Append(buffer.Span[..read]);
        return read;
    }

    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        var read = await inner.ReadAsync(
            buffer.AsMemory(offset, count),
            cancellationToken);
        Append(buffer.AsSpan(offset, read));
        return read;
    }

    public override void Flush() =>
        throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    public override void SetLength(long value) =>
        throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            hash.Dispose();
        }

        base.Dispose(disposing);
    }

    private void Append(ReadOnlySpan<byte> bytes)
    {
        if (!bytes.IsEmpty)
        {
            hash.AppendData(bytes);
        }
    }
}
