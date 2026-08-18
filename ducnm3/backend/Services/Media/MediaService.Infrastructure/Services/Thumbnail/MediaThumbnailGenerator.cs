// File: backend/Services/Media/MediaService.Infrastructure/Services/Thumbnail/MediaThumbnailGenerator.cs
// Mục đích: Adapter hạ tầng tạo thumbnail ảnh/video; tải source, gọi công cụ xử lý và trả metadata thumbnail đã tạo.

using System.Globalization;
using MediaService.Application.Services.Derivation;
using MediaService.Application.UseCases.MediaDerivations.GenerateThumbnail;
using MediaService.Domain.Constants;
using SkiaSharp;

namespace MediaService.Infrastructure.Services.Thumbnail;

public sealed class MediaThumbnailGenerator : IThumbnailGenerator
{
    private readonly MediaThumbnailOptions options;
    private readonly NativeProcessRunner processRunner;

    public MediaThumbnailGenerator(MediaThumbnailOptions options)
    {
        this.options = options;
        processRunner = new NativeProcessRunner(
            TimeSpan.FromSeconds(options.ProcessTimeoutSeconds));
    }

    public async Task<GeneratedThumbnail> GenerateAsync(
        ThumbnailGenerationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var rasterPath = request.SourcePath;
        var deleteRaster = false;

        try
        {
            if (string.Equals(
                    request.MediaType,
                    MediaTypes.Video,
                    StringComparison.Ordinal))
            {
                rasterPath = await CaptureVideoFrameAsync(
                    request.SourcePath,
                    cancellationToken);
                deleteRaster = true;
            }
            else if (string.Equals(
                         request.MediaType,
                         MediaTypes.Document,
                         StringComparison.Ordinal) &&
                     string.Equals(
                         request.ContentType,
                         "application/pdf",
                         StringComparison.OrdinalIgnoreCase))
            {
                rasterPath = await RenderPdfFirstPageAsync(
                    request.SourcePath,
                    cancellationToken);
                deleteRaster = true;
            }
            else if (!string.Equals(
                         request.MediaType,
                         MediaTypes.Image,
                         StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The source media type does not support thumbnail generation.");
            }

            return EncodeWebp(rasterPath);
        }
        finally
        {
            if (deleteRaster)
            {
                TryDelete(rasterPath);
            }
        }
    }

    private async Task<string> CaptureVideoFrameAsync(
        string sourcePath,
        CancellationToken cancellationToken)
    {
        var durationOutput = await processRunner.RunAsync(
            "ffprobe",
            [
                "-v", "error",
                "-show_entries", "format=duration",
                "-of", "default=noprint_wrappers=1:nokey=1",
                sourcePath,
            ],
            cancellationToken);
        if (!double.TryParse(
                durationOutput.Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var durationSeconds) ||
            durationSeconds <= 0)
        {
            throw new InvalidOperationException(
                "The video duration could not be determined.");
        }

        var captureSeconds = Math.Clamp(
            durationSeconds * options.VideoCapturePercent,
            options.VideoCaptureMinSeconds,
            options.VideoCaptureMaxSeconds);
        if (captureSeconds >= durationSeconds)
        {
            captureSeconds = Math.Max(0, durationSeconds * options.VideoCapturePercent);
        }

        var outputPath = CreateSiblingPath(sourcePath, ".video-frame.png");
        await processRunner.RunAsync(
            "ffmpeg",
            [
                "-v", "error",
                "-ss", captureSeconds.ToString("0.###", CultureInfo.InvariantCulture),
                "-i", sourcePath,
                "-frames:v", "1",
                "-y",
                outputPath,
            ],
            cancellationToken);
        EnsureOutputExists(outputPath);
        return outputPath;
    }

    private async Task<string> RenderPdfFirstPageAsync(
        string sourcePath,
        CancellationToken cancellationToken)
    {
        var outputPrefix = CreateSiblingPath(sourcePath, ".pdf-page");
        var outputPath = $"{outputPrefix}.png";
        await processRunner.RunAsync(
            "pdftoppm",
            [
                "-f", "1",
                "-l", "1",
                "-singlefile",
                "-png",
                sourcePath,
                outputPrefix,
            ],
            cancellationToken);
        EnsureOutputExists(outputPath);
        return outputPath;
    }

    private GeneratedThumbnail EncodeWebp(string rasterPath)
    {
        using var source = SKBitmap.Decode(rasterPath) ??
            throw new InvalidOperationException(
                "The source image could not be decoded.");
        if (source.Width <= 0 || source.Height <= 0)
        {
            throw new InvalidOperationException(
                "The source image dimensions are invalid.");
        }

        var scale = Math.Min(
            1D,
            Math.Min(
                (double)options.MaxWidth / source.Width,
                (double)options.MaxHeight / source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        using var resized = new SKBitmap(
            width,
            height,
            SKColorType.Rgba8888,
            SKAlphaType.Premul);
        using (var canvas = new SKCanvas(resized))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(
                source,
                new SKRect(0, 0, width, height),
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        }

        using var image = SKImage.FromBitmap(resized);
        using var encoded = image.Encode(
            SKEncodedImageFormat.Webp,
            options.WebpQuality);
        if (encoded is null)
        {
            throw new InvalidOperationException(
                "The thumbnail could not be encoded as WebP.");
        }

        return new GeneratedThumbnail(encoded.ToArray(), width, height);
    }

    private static string CreateSiblingPath(string sourcePath, string suffix) =>
        System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(sourcePath) ??
                System.IO.Path.GetTempPath(),
            $"{Guid.NewGuid():N}{suffix}");

    private static void EnsureOutputExists(string path)
    {
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
        {
            throw new InvalidOperationException(
                "The media tool did not produce a thumbnail frame.");
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (FileNotFoundException)
        {
        }
    }
}
