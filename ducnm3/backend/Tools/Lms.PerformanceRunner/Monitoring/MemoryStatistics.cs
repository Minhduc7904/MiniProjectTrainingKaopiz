namespace Lms.PerformanceRunner.Monitoring;

public sealed record MemoryStatistics(
    long BaselineBytes,
    long PeakBytes,
    double AverageBytes,
    long DeltaBytes,
    int SampleCount)
{
    public double BaselineMegabytes => ToMegabytes(BaselineBytes);

    public double PeakMegabytes => ToMegabytes(PeakBytes);

    public double AverageMegabytes => ToMegabytes(AverageBytes);

    public double DeltaMegabytes => ToMegabytes(DeltaBytes);

    public static MemoryStatistics Calculate(
        IReadOnlyList<ContainerStatsSample> samples,
        int minimumBaselineSamples = 5)
    {
        ArgumentNullException.ThrowIfNull(samples);
        if (samples.Count == 0)
        {
            throw new ArgumentException("At least one memory sample is required.", nameof(samples));
        }

        if (minimumBaselineSamples < 1 || minimumBaselineSamples > samples.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumBaselineSamples));
        }

        var baselineValues = samples
            .Take(minimumBaselineSamples)
            .Select(sample => sample.MemoryBytes)
            .Order()
            .ToArray();
        var baseline = baselineValues.Length % 2 == 1
            ? baselineValues[baselineValues.Length / 2]
            : (baselineValues[baselineValues.Length / 2 - 1] + baselineValues[baselineValues.Length / 2]) / 2;
        var peak = samples.Max(sample => sample.MemoryBytes);
        var average = samples.Average(sample => (double)sample.MemoryBytes);

        return new MemoryStatistics(baseline, peak, average, peak - baseline, samples.Count);
    }

    private static double ToMegabytes(double bytes) => bytes / (1024d * 1024d);
}