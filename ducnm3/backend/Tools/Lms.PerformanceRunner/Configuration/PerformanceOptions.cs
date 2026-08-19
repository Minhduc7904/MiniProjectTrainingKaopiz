namespace Lms.PerformanceRunner.Configuration;

public enum PerformanceCommand
{
    Batch,
    Csv,
}

public enum CsvApproach
{
    Buffered,
    Streaming,
    Both,
}

public sealed record PerformanceOptions(
    PerformanceCommand Command,
    IReadOnlyList<int> RecipientCounts,
    IReadOnlyList<int> RecordCounts,
    CsvApproach Approach,
    Uri GatewayUrl,
    string OutputDirectory,
    int WarmupRuns,
    int MeasuredRuns,
    int PollIntervalMilliseconds,
    int MemorySampleIntervalMilliseconds,
    TimeSpan Timeout,
    bool PrepareData,
    bool ConfirmReset,
    bool PlainOutput);

public sealed class PerformanceArgumentException(string message) : Exception(message);
