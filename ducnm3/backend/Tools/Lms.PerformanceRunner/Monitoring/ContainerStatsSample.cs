namespace Lms.PerformanceRunner.Monitoring;

public sealed record ContainerStatsSample(
    DateTimeOffset TimestampUtc,
    TimeSpan Elapsed,
    string Service,
    string ContainerId,
    long MemoryBytes,
    double? CpuPercent = null);