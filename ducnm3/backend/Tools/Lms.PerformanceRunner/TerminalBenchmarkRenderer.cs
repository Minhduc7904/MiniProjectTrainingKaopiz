using Lms.PerformanceRunner.Http;
using Lms.PerformanceRunner.Monitoring;
using Lms.PerformanceRunner.Models;
using Spectre.Console;
using System.Globalization;

namespace Lms.PerformanceRunner;

public sealed class TerminalBenchmarkRenderer(bool enabled)
{
    public async Task<(CsvExportMeasurement Measurement, ContainerStatsCapture Capture)> RunCsvAsync(
        CourseBenchmarkClient client,
        ContainerStatsMonitor monitor,
        string approach,
        string? status,
        int records,
        int run,
        int totalRuns,
        CancellationToken cancellationToken)
    {
        CsvExportMeasurement? measurement = null;
        ContainerStatsCapture? capture = null;
        var state = new CsvState(approach, records, run, totalRuns);

        async Task ExecuteAsync(IProgress<CsvDownloadProgress>? downloadProgress, IProgress<ContainerStatsSample>? resourceProgress)
        {
            capture = await monitor.CaptureAsync(async token =>
            {
                measurement = await client.DownloadAsync(approach, status, token, downloadProgress, records);
            }, cancellationToken, resourceProgress);
        }

        if (!enabled)
        {
            await ExecuteAsync(null, null);
        }
        else
        {
            await AnsiConsole.Live(RenderCsv(state)).StartAsync(async context =>
            {
                var progress = new Progress<CsvDownloadProgress>(value =>
                {
                    state.Progress = value;
                    context.UpdateTarget(RenderCsv(state));
                });
                var resource = new Progress<ContainerStatsSample>(value =>
                {
                    state.Resource = value;
                    context.UpdateTarget(RenderCsv(state));
                });
                await ExecuteAsync(progress, resource);
                state.Completed = true;
                context.UpdateTarget(RenderCsv(state));
            });
        }

        return (measurement ?? throw new InvalidOperationException("CSV measurement was not produced."),
            capture ?? throw new InvalidOperationException("CSV resource capture was not produced."));
    }

    public async Task<(BatchBenchmarkResult Result, ContainerStatsCapture Capture)> RunBatchAsync(
        NotificationBenchmarkClient client,
        ContainerStatsMonitor monitor,
        int users,
        int run,
        int totalRuns,
        int pollIntervalMilliseconds,
        CancellationToken cancellationToken)
    {
        BatchBenchmarkResult? result = null;
        ContainerStatsCapture? capture = null;
        var state = new BatchState(users, run, totalRuns);

        async Task ExecuteAsync(IProgress<BatchDownloadProgress>? batchProgress, IProgress<ContainerStatsSample>? resourceProgress)
        {
            capture = await monitor.CaptureAsync(async token =>
            {
                result = await client.RunAsync(users, pollIntervalMilliseconds, token, batchProgress);
            }, cancellationToken, resourceProgress);
        }

        if (!enabled)
        {
            await ExecuteAsync(null, null);
        }
        else
        {
            await AnsiConsole.Live(RenderBatch(state)).StartAsync(async context =>
            {
                var progress = new Progress<BatchDownloadProgress>(value =>
                {
                    state.Progress = value;
                    context.UpdateTarget(RenderBatch(state));
                });
                var resource = new Progress<ContainerStatsSample>(value =>
                {
                    state.Resource = value;
                    context.UpdateTarget(RenderBatch(state));
                });
                await ExecuteAsync(progress, resource);
                state.Completed = true;
                context.UpdateTarget(RenderBatch(state));
            });
        }

        return (result ?? throw new InvalidOperationException("Batch result was not produced."),
            capture ?? throw new InvalidOperationException("Batch resource capture was not produced."));
    }

    private static Table RenderCsv(CsvState state)
    {
        var progress = state.Progress;
        var percent = progress?.TotalBytes is > 0
            ? Math.Clamp(progress.BytesRead / (double)progress.TotalBytes.Value * 100, 0, 100)
            : 0;
        var speed = progress is null || progress.Elapsed == TimeSpan.Zero
            ? 0
            : progress.BytesRead / progress.Elapsed.TotalSeconds;
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
        table.AddColumn(new TableColumn("CSV export").LeftAligned());
        table.AddColumn(new TableColumn("Value").RightAligned());
        table.AddRow("Dataset", $"{state.Records:N0} records | {state.Approach} | run {state.Run}/{state.TotalRuns}");
        table.AddRow("Progress", progress?.TotalBytes is > 0 ? $"{percent:0.0}%  {FormatBytes(progress.BytesRead)} / {FormatBytes(progress.TotalBytes!.Value)}" : $"{FormatBytes(progress?.BytesRead ?? 0)} downloaded");
        table.AddRow("Rows", $"{progress?.RowsRead ?? 0:N0}");
        table.AddRow("Speed", $"{FormatBytes((long)speed)}/s");
        table.AddRow("Elapsed", FormatDuration(progress?.Elapsed ?? TimeSpan.Zero));
        table.AddRow("TTFB", FormatDuration(progress?.Ttfb));
        AddResourceRows(table, state.Resource);
        table.AddRow("Status", state.Completed ? "[green]Completed[/]" : "[yellow]Downloading...[/]");
        return table;
    }

    private static Table RenderBatch(BatchState state)
    {
        var progress = state.Progress;
        var percent = progress?.TotalCount > 0
            ? Math.Clamp(progress.ProcessedCount / (double)progress.TotalCount * 100, 0, 100)
            : 0;
        var rate = progress is null || progress.Elapsed == TimeSpan.Zero
            ? 0
            : progress.ProcessedCount / progress.Elapsed.TotalSeconds;
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Green1);
        table.AddColumn(new TableColumn("Notification batch").LeftAligned());
        table.AddColumn(new TableColumn("Value").RightAligned());
        table.AddRow("Dataset", $"{state.Users:N0} users | run {state.Run}/{state.TotalRuns}");
        table.AddRow("Phase", progress?.Phase ?? "starting");
        table.AddRow("Status", progress?.Status ?? "CREATING");
        table.AddRow("Progress", progress?.TotalCount > 0 ? $"{percent:0.0}%  {progress.ProcessedCount:N0} / {progress.TotalCount:N0}" : "waiting for counters");
        table.AddRow("Success / failed", $"{progress?.SuccessCount ?? 0:N0} / {progress?.FailedCount ?? 0:N0}");
        table.AddRow("Rate", $"{rate:0} users/s");
        table.AddRow("Elapsed", FormatDuration(progress?.Elapsed ?? TimeSpan.Zero));
        AddResourceRows(table, state.Resource);
        table.AddRow("Status", state.Completed ? "[green]Completed[/]" : "[yellow]Polling...[/]");
        return table;
    }

    private static void AddResourceRows(Table table, ContainerStatsSample? sample)
    {
        table.AddRow("CPU", sample?.CpuPercent is double cpu ? $"{cpu:0.0}%" : "n/a");
        table.AddRow("RAM", sample is null ? "n/a" : FormatBytes(sample.MemoryBytes));
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        var value = (double)Math.Max(0, bytes);
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.0} {units[unit]}";
    }

    private static string FormatDuration(TimeSpan? duration) =>
        duration is null ? "n/a" : duration.Value.ToString("hh\\:mm\\:ss\\.ff", CultureInfo.InvariantCulture);

    private sealed class CsvState(string approach, int records, int run, int totalRuns)
    {
        public string Approach { get; } = approach;
        public int Records { get; } = records;
        public int Run { get; } = run;
        public int TotalRuns { get; } = totalRuns;
        public CsvDownloadProgress? Progress { get; set; }
        public ContainerStatsSample? Resource { get; set; }
        public bool Completed { get; set; }
    }

    private sealed class BatchState(int users, int run, int totalRuns)
    {
        public int Users { get; } = users;
        public int Run { get; } = run;
        public int TotalRuns { get; } = totalRuns;
        public BatchDownloadProgress? Progress { get; set; }
        public ContainerStatsSample? Resource { get; set; }
        public bool Completed { get; set; }
    }
}
