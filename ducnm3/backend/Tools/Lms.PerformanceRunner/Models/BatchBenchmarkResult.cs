namespace Lms.PerformanceRunner.Models;

public sealed record BatchBenchmarkResult(
    Guid BatchId,
    TimeSpan PostLatency,
    TimeSpan TotalDuration,
    TimeSpan? SnapshotDuration,
    int SnapshotTimingErrorMilliseconds,
    long? DispatchDurationMilliseconds,
    string FinalStatus,
    uint TotalCount,
    uint ProcessedCount,
    uint SuccessCount,
    uint FailedCount,
    double EndToEndThroughput,
    double? DispatchThroughput);