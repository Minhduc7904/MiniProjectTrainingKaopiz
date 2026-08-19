// File: backend/Services/Media/MediaService.UnitTests/Api/Endpoints/Media/MediaRequestParserTests.cs
// Mục đích: Khai báo các route HTTP của MediaRequestParserTests, chuyển request đến use case và chuẩn hóa HTTP response.

using System.Text;
using BuildingBlocks.Contracts.Api;
using MediaService.Api.Endpoints.Media;
using MediaService.Application;
using MediaService.Application.Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

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
