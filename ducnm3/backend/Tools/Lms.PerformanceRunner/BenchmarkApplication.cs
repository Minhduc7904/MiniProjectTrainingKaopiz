using System.Text.Json;
using System.Text.Json.Serialization;
using Lms.PerformanceRunner.Configuration;
using Lms.PerformanceRunner.Http;
using Lms.PerformanceRunner.Monitoring;
using Lms.PerformanceRunner.Models;

namespace Lms.PerformanceRunner;

public sealed class BenchmarkApplication
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task RunAsync(PerformanceOptions options, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient { BaseAddress = options.GatewayUrl, Timeout = options.Timeout };
        var outputDirectory = Path.GetFullPath(options.OutputDirectory);
        Directory.CreateDirectory(outputDirectory);
        await WriteMetadataAsync(outputDirectory, options, cancellationToken);

        if (options.Command == PerformanceCommand.Batch)
        {
            var client = new NotificationBenchmarkClient(httpClient);
            foreach (var count in options.RecipientCounts)
            {
                await RunBatchCountAsync(client, count, options, outputDirectory, cancellationToken);
            }

            return;
        }

        var csvClient = new CourseBenchmarkClient(httpClient);
        foreach (var count in options.RecordCounts)
        {
            await RunCsvCountAsync(csvClient, count, options, outputDirectory, cancellationToken);
        }
    }

    private static async Task RunBatchCountAsync(
        NotificationBenchmarkClient client,
        int count,
        PerformanceOptions options,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        for (var run = 0; run < options.WarmupRuns; run++)
        {
            await client.RunAsync(count, options.PollIntervalMilliseconds, cancellationToken);
            Console.WriteLine($"warmup batch users={count} run={run + 1}");
        }

        var results = new List<object>();
        for (var run = 0; run < options.MeasuredRuns; run++)
        {
            var monitor = CreateMonitor("notification-worker", options);
            ContainerStatsCapture? capture = null;
            BatchBenchmarkResult? result = null;
            capture = await monitor.CaptureAsync(async token =>
            {
                result = await client.RunAsync(count, options.PollIntervalMilliseconds, token);
            }, cancellationToken);
            results.Add(new { DatasetCount = count, Run = run + 1, Result = result, Memory = capture.Memory, Samples = capture.Samples });
            Console.WriteLine($"batch users={count} run={run + 1} status={result!.FinalStatus} totalMs={result.TotalDuration.TotalMilliseconds:0}");
        }

        await WriteJsonAsync(Path.Combine(outputDirectory, $"batch-{count}.json"), results, cancellationToken);
    }

    private static async Task RunCsvCountAsync(
        CourseBenchmarkClient client,
        int count,
        PerformanceOptions options,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        string[] approaches = options.Approach switch
        {
            CsvApproach.Buffered => ["buffered"],
            CsvApproach.Streaming => ["streaming"],
            _ => ["buffered", "streaming"],
        };
        for (var run = 0; run < options.WarmupRuns; run++)
        {
            foreach (var approach in approaches)
            {
                await client.DownloadAsync(approach, null, cancellationToken);
            }

            Console.WriteLine($"warmup csv records={count} run={run + 1}");
        }

        var results = new List<object>();
        for (var run = 0; run < options.MeasuredRuns; run++)
        {
            var measured = new List<CsvExportMeasurement>();
            var memory = new List<MemoryStatistics>();
            foreach (var approach in approaches)
            {
                var monitor = CreateMonitor("course-service", options);
                CsvExportMeasurement? measurement = null;
                var capture = await monitor.CaptureAsync(async token =>
                {
                    measurement = await client.DownloadAsync(approach, null, token);
                }, cancellationToken);
                measured.Add(measurement!);
                memory.Add(capture.Memory);
            }

            if (measured.Count == 2 && measured[0].ContentSha256 != measured[1].ContentSha256)
            {
                throw new InvalidOperationException("Buffered and streaming CSV content hashes differ.");
            }

            results.Add(new { DatasetCount = count, Run = run + 1, Measurements = measured, Memory = memory });
            Console.WriteLine($"csv records={count} run={run + 1} approaches={string.Join(',', approaches)}");
        }

        await WriteJsonAsync(Path.Combine(outputDirectory, $"csv-{count}.json"), results, cancellationToken);
    }

    private static Task WriteMetadataAsync(
        string outputDirectory,
        PerformanceOptions options,
        CancellationToken cancellationToken) =>
        WriteJsonAsync(Path.Combine(outputDirectory, "metadata.json"), new
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            Options = options,
            Note = "Raw benchmark results only; preparation and container samples are not included yet.",
        }, cancellationToken);

    private static ContainerStatsMonitor CreateMonitor(string service, PerformanceOptions options) =>
        new(
            FindRepositoryRoot(),
            service,
            TimeSpan.FromMilliseconds(options.MemorySampleIntervalMilliseconds));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "docker-compose.yml"))) return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root containing docker-compose.yml.");
    }

    private static async Task WriteJsonAsync(
        string path,
        object value,
        CancellationToken cancellationToken)
    {
        var temporaryPath = path + ".tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, value, JsonOptions, cancellationToken);
        }

        File.Move(temporaryPath, path, true);
    }
}