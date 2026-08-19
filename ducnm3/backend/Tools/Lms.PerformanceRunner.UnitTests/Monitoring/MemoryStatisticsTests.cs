using Lms.PerformanceRunner.Monitoring;

namespace Lms.PerformanceRunner.UnitTests.Monitoring;

public sealed class MemoryStatisticsTests
{
    [Test]
    public void CalculateUsesMedianBaselineAndMeasurementAggregates()
    {
        var samples = new[]
        {
            Sample(10), Sample(14), Sample(12), Sample(16), Sample(18), Sample(100),
        };

        var statistics = MemoryStatistics.Calculate(samples);

        Assert.Multiple(() =>
        {
            Assert.That(statistics.BaselineBytes, Is.EqualTo(14 * 1024 * 1024));
            Assert.That(statistics.PeakBytes, Is.EqualTo(100 * 1024 * 1024));
            Assert.That(statistics.AverageBytes, Is.EqualTo(28.3333333333 * 1024 * 1024).Within(1));
            Assert.That(statistics.DeltaBytes, Is.EqualTo(86 * 1024 * 1024));
            Assert.That(statistics.SampleCount, Is.EqualTo(6));
        });
    }

    [Test]
    public void CalculateRejectsFewerThanRequiredBaselineSamples()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MemoryStatistics.Calculate([Sample(10)], minimumBaselineSamples: 5));
    }

    private static ContainerStatsSample Sample(int megabytes) => new(
        DateTimeOffset.UtcNow,
        TimeSpan.Zero,
        "course-service",
        "container-id",
        megabytes * 1024L * 1024L);
}