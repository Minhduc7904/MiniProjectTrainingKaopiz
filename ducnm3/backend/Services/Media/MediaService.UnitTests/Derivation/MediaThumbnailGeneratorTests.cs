// File: backend/Services/Media/MediaService.UnitTests/Derivation/MediaThumbnailGeneratorTests.cs
// Mục đích: Kiểm thử adapter tạo thumbnail để bảo đảm media derivation sinh đúng kết quả và quy đổi lỗi chính xác.

using MediaService.Application.Services.Derivation;
using MediaService.Application.UseCases.MediaDerivations.GenerateThumbnail;
using MediaService.Domain.Constants;
using MediaService.Infrastructure.Services.Thumbnail;
using SkiaSharp;

namespace MediaService.UnitTests.Derivation;

public class MediaThumbnailGeneratorTests
{
    [Test]
    public async Task ImageIsFitWithinPresetAndEncodedAsWebp()
    {
        var sourcePath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new SKBitmap(1280, 720))
            {
                bitmap.Erase(SKColors.CornflowerBlue);
                using var image = SKImage.FromBitmap(bitmap);
                using var png = image.Encode(SKEncodedImageFormat.Png, 100);
                await File.WriteAllBytesAsync(sourcePath, png.ToArray());
            }

            var generator = new MediaThumbnailGenerator(
                new MediaThumbnailOptions
                {
                    MaxWidth = 640,
                    MaxHeight = 640,
                    WebpQuality = 80,
                });

            var result = await generator.GenerateAsync(
                new ThumbnailGenerationRequest(
                    sourcePath,
                    MediaTypes.Image,
                    "image/png"),
                CancellationToken.None);

            using var decoded = SKBitmap.Decode(result.Content);
            Assert.Multiple(() =>
            {
                Assert.That(decoded, Is.Not.Null);
                Assert.That(result.Width, Is.EqualTo(640));
                Assert.That(result.Height, Is.EqualTo(360));
                Assert.That(decoded!.Width, Is.EqualTo(640));
                Assert.That(decoded.Height, Is.EqualTo(360));
                Assert.That(result.Content, Is.Not.Empty);
            });
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    [TestCase(MediaTypes.Image, "image/png", true)]
    [TestCase(MediaTypes.Video, "video/mp4", true)]
    [TestCase(MediaTypes.Document, "application/pdf", true)]
    [TestCase(MediaTypes.Document, "text/plain", false)]
    [TestCase(MediaTypes.Audio, "audio/mpeg", false)]
    public void PolicyOnlyQueuesSupportedMedia(
        string mediaType,
        string contentType,
        bool expected)
    {
        Assert.That(
            MediaThumbnailPolicy.RequiresThumbnail(mediaType, contentType),
            Is.EqualTo(expected));
    }
}
