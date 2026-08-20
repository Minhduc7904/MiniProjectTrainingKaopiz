namespace Lms.PerformanceRunner.Monitoring;

public sealed record ResourceStatistics(
    double CurrentCpuPercent,
    double PeakCpuPercent,
    double AverageCpuPercent,
    int SampleCount)
{
    public static ResourceStatistics Calculate(IReadOnlyList<ContainerStatsSample> samples)
    {
        ArgumentNullException.ThrowIfNull(samples);
        var cpuValues = samples
            .Where(sample => sample.CpuPercent.HasValue)
            .Select(sample => sample.CpuPercent!.Value)
            .ToArray();
        if (cpuValues.Length == 0)
        {
            return new ResourceStatistics(0, 0, 0, samples.Count);
        }

        return new ResourceStatistics(
            cpuValues[^1],
            cpuValues.Max(),
            cpuValues.Average(),
            samples.Count);
    }
}