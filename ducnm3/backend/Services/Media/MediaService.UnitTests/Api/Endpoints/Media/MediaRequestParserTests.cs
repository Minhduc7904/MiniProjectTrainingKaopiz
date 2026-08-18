// File: backend/Services/Media/MediaService.UnitTests/Api/Endpoints/Media/MediaRequestParserTests.cs
// Mục đích: Khai báo Minimal API endpoint tại HTTP boundary của Media Service.

using System.Text;
using BuildingBlocks.Contracts.Api;
using MediaService.Api.Endpoints.Media;
using MediaService.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using MediaService.Application.Common.Errors;

namespace MediaService.UnitTests.Endpoints;

public class MediaRequestParserTests
{
    [Test]
    public void MultipartLengthLimitReturnsPayloadTooLarge()
    {
        const string boundary = "media-test-boundary";
        var body = string.Join(
            "\r\n",
            $"--{boundary}",
            "Content-Disposition: form-data; name=\"file\"; filename=\"file.png\"",
            "Content-Type: image/png",
            string.Empty,
            "too-large",
            $"--{boundary}--",
            string.Empty);
        var context = new DefaultHttpContext();
        context.Request.ContentType = $"multipart/form-data; boundary={boundary}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Features.Set<IFormFeature>(
            new FormFeature(
                context.Request,
                new FormOptions { MultipartBodyLengthLimit = 1 }));

        var exception = Assert.ThrowsAsync<MediaApplicationException>(
            () => MediaRequestParser.ReadMultipartFormAsync(
                context.Request,
                CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(413));
            Assert.That(
                exception.ErrorCode,
                Is.EqualTo(ApiErrorCodes.PayloadTooLarge));
        });
    }
}
