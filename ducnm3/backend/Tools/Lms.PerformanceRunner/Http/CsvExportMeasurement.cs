namespace Lms.PerformanceRunner.Http;

public sealed record CsvExportMeasurement(
    string Approach,
    TimeSpan ResponseHeadersTime,
    TimeSpan? Ttfb,
    TimeSpan TotalDownloadTime,
    long ResponseBytes,
    long RowsReceived,
    string ContentSha256);