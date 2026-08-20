namespace Lms.PerformanceRunner.Http;

public sealed record CsvDownloadProgress(
    string Approach,
    long BytesRead,
    long? TotalBytes,
    long RowsRead,
    TimeSpan Elapsed,
    TimeSpan? Ttfb);