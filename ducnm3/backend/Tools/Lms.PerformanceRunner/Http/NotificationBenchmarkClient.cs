using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using Lms.PerformanceRunner.Models;

namespace Lms.PerformanceRunner.Http;

public sealed class NotificationBenchmarkClient(HttpClient httpClient)
{
    private static readonly Guid BenchmarkCreatedBy =
        Guid.Parse("00000000-0000-0000-0000-000000000305");

    public async Task<BatchBenchmarkResult> RunAsync(
        int recipientCount,
        int pollIntervalMilliseconds,
        CancellationToken cancellationToken,
        IProgress<BatchDownloadProgress>? progress = null)
    {
        var stopwatch = Stopwatch.StartNew();
        using var createResponse = await httpClient.PostAsJsonAsync(
            ApiRoutes.Notifications.BatchesPublicPath(),
            new
            {
                title = $"Performance benchmark {recipientCount}",
                bodyMarkdown = "Performance benchmark notification.",
                targetScope = "ALL_STUDENTS",
                createdBy = BenchmarkCreatedBy,
                batchSize = 500,
                requestedCount = recipientCount,
                courseId = (Guid?)null,
            },
            cancellationToken);
        var postLatency = stopwatch.Elapsed;
        await EnsureSuccessAsync(createResponse, "create batch", cancellationToken);
        using var createDocument = await JsonDocument.ParseAsync(
            await createResponse.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
        var data = createDocument.RootElement.GetProperty("data");
        var batchId = data.GetProperty("id").GetGuid();
        progress?.Report(new BatchDownloadProgress(
            "snapshot",
            "ACCEPTED",
            0,
            0,
            0,
            0,
            stopwatch.Elapsed));

        TimeSpan? snapshotDuration = null;
        while (true)
        {
            using var snapshot = await GetStatusAsync(
                ApiRoutes.Notifications.BatchSnapshotStatusPublicPath(batchId),
                cancellationToken);
            await EnsureSuccessAsync(snapshot, "snapshot status", cancellationToken);
            using var snapshotDocument = await JsonDocument.ParseAsync(
                await snapshot.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            var snapshotData = snapshotDocument.RootElement.GetProperty("data");
            var snapshotStatus = snapshotData.GetProperty("status").GetString();
            progress?.Report(new BatchDownloadProgress(
                "snapshot",
                snapshotStatus ?? "UNKNOWN",
                GetUInt32(snapshotData, "totalCount"),
                GetUInt32(snapshotData, "processedCount"),
                GetUInt32(snapshotData, "successCount"),
                GetUInt32(snapshotData, "failedCount"),
                stopwatch.Elapsed));
            if (snapshotStatus is "COMPLETED" or "FAILED")
            {
                snapshotDuration = stopwatch.Elapsed - postLatency;
                if (snapshotStatus == "FAILED")
                {
                    throw new InvalidOperationException("Notification batch snapshot failed.");
                }

                break;
            }

            await Task.Delay(pollIntervalMilliseconds, cancellationToken);
        }

        while (true)
        {
            using var delivery = await GetStatusAsync(
                ApiRoutes.Notifications.BatchDeliveryStatusPublicPath(batchId),
                cancellationToken);
            await EnsureSuccessAsync(delivery, "delivery status", cancellationToken);
            using var deliveryDocument = await JsonDocument.ParseAsync(
                await delivery.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            var deliveryData = deliveryDocument.RootElement.GetProperty("data");
            var status = deliveryData.GetProperty("status").GetString() ?? "UNKNOWN";
            var totalCount = deliveryData.GetProperty("totalCount").GetUInt32();
            var processedCount = deliveryData.GetProperty("processedCount").GetUInt32();
            var successCount = deliveryData.GetProperty("successCount").GetUInt32();
            var failedCount = deliveryData.GetProperty("failedCount").GetUInt32();
            progress?.Report(new BatchDownloadProgress(
                "delivery",
                status,
                totalCount,
                processedCount,
                successCount,
                failedCount,
                stopwatch.Elapsed));
            if (status is not ("COMPLETED" or "FAILED"))
            {
                await Task.Delay(pollIntervalMilliseconds, cancellationToken);
                continue;
            }

            long? dispatchDuration = deliveryData.TryGetProperty("durationMs", out var duration)
                && duration.ValueKind != JsonValueKind.Null
                ? duration.GetInt64()
                : null;
            var totalSeconds = Math.Max(stopwatch.Elapsed.TotalSeconds, double.Epsilon);
            var dispatchSeconds = dispatchDuration is > 0 ? dispatchDuration.Value / 1000d : 0;
            return new BatchBenchmarkResult(
                batchId,
                postLatency,
                stopwatch.Elapsed,
                snapshotDuration,
                pollIntervalMilliseconds,
                dispatchDuration,
                status,
                totalCount,
                processedCount,
                successCount,
                failedCount,
                processedCount / totalSeconds,
                dispatchSeconds > 0 ? processedCount / dispatchSeconds : null);
        }
    }

    private Task<HttpResponseMessage> GetStatusAsync(string path, CancellationToken cancellationToken) =>
        httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

    private static uint GetUInt32(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.GetUInt32()
            : 0;

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string stage,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Notification {stage} returned {(int)response.StatusCode}: {detail[..Math.Min(detail.Length, 256)]}",
            null,
            response.StatusCode);
    }
}