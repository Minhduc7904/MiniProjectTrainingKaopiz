// File: backend/Services/Course/CourseService.Infrastructure/Clients/Media/MediaContentReader.cs
// Mục đích: Tải nội dung tệp từ GET /api/media/{mediaId}/content của Media Service với giới hạn kích thước.

using System.Net;
using BuildingBlocks.Contracts.Api;
using CourseService.Application.Services.Media;

namespace CourseService.Infrastructure.Clients.Media;

public sealed class MediaContentReader(HttpClient httpClient) : IMediaContentReader
{
    public async Task<byte[]> ReadAsync(Guid mediaId, long maxBytes, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(ApiRoutes.Media.ContentServicePath(mediaId), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new MediaContentException(MediaContentException.Unavailable, "Media Service did not respond in time.");
        }
        catch (HttpRequestException)
        {
            throw new MediaContentException(MediaContentException.Unavailable, "Media Service is unavailable.");
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new MediaContentException(MediaContentException.NotFound, "The attached file no longer exists.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new MediaContentException(MediaContentException.Unavailable, $"Media Service returned HTTP {(int)response.StatusCode}.");
            }

            if (response.Content.Headers.ContentLength > maxBytes)
            {
                throw TooLarge(maxBytes);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;
            while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
            {
                if (buffer.Length + read > maxBytes) throw TooLarge(maxBytes);
                buffer.Write(chunk, 0, read);
            }

            return buffer.ToArray();
        }
    }

    private static MediaContentException TooLarge(long maxBytes) =>
        new(MediaContentException.TooLarge, $"The file is larger than the {maxBytes / (1024 * 1024)} MB limit for AI processing.");
}
