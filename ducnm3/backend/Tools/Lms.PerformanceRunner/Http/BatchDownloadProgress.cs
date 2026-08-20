namespace Lms.PerformanceRunner.Http;

public sealed record BatchDownloadProgress(
    string Phase,
    string Status,
    uint TotalCount,
    uint ProcessedCount,
    uint SuccessCount,
    uint FailedCount,
    TimeSpan Elapsed);