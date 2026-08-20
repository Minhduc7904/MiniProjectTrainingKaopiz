using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Contracts.Api;

namespace Lms.PerformanceRunner.Http;

public sealed class CourseBenchmarkClient(HttpClient httpClient)
{
    public async Task<CsvExportMeasurement> DownloadAsync(
        string approach,
        string? status,
        CancellationToken cancellationToken,
        IProgress<CsvDownloadProgress>? progress = null,
        int? limit = null)
    {
        var path = approach switch
        {
            "buffered" => ApiRoutes.Courses.BufferedExportBenchmarkPublicPath(),
            "streaming" => ApiRoutes.Courses.ExportPublicPath(),
            _ => throw new ArgumentOutOfRangeException(nameof(approach)),
        };
        var uri = BuildUri(path, status, limit);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var stopwatch = Stopwatch.StartNew();
        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var headersTime = stopwatch.Elapsed;
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"CSV {approach} endpoint returned {(int)response.StatusCode} {response.StatusCode}.",
                null,
                response.StatusCode);
        }

        await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[64 * 1024];
        var bytes = 0L;
        var rows = 0L;
        var totalBytes = response.Content.Headers.ContentLength;
        TimeSpan? ttfb = null;
        var previousByte = (byte)0;
        var hasByte = false;
        while (true)
        {
            var read = await body.ReadAsync(buffer, cancellationToken);
            if (read == 0) break;

            ttfb ??= stopwatch.Elapsed;
            hash.AppendData(buffer, 0, read);
            bytes += read;
            for (var index = 0; index < read; index++)
            {
                if (buffer[index] == (byte)'\n' && hasByte && previousByte == (byte)'\r') rows++;
                previousByte = buffer[index];
                hasByte = true;
            }

            progress?.Report(new CsvDownloadProgress(
                approach,
                bytes,
                totalBytes,
                rows,
                stopwatch.Elapsed,
                ttfb));
        }

        var totalTime = stopwatch.Elapsed;
        return new CsvExportMeasurement(
            approach,
            headersTime,
            ttfb,
            totalTime,
            bytes,
            Math.Max(0, rows - 1),
            Convert.ToHexString(hash.GetHashAndReset()));
    }

    private Uri BuildUri(string path, string? status, int? limit)
    {
        var builder = new UriBuilder(new Uri(httpClient.BaseAddress ?? throw new InvalidOperationException("HttpClient.BaseAddress is required."), path));
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query.Add($"status={Uri.EscapeDataString(status)}");
        }
        if (limit is not null) query.Add($"limit={limit.Value}");
        builder.Query = string.Join('&', query);

        return builder.Uri;
    }
}