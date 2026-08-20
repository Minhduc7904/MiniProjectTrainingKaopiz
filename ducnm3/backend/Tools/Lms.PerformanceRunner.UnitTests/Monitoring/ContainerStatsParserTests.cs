using Lms.PerformanceRunner.Monitoring;

namespace Lms.PerformanceRunner.UnitTests.Monitoring;

public sealed class ContainerStatsParserTests
{
    [TestCase("0.0%", 0)]
    [TestCase("72.4%", 72.4)]
    public void ParseCpuPercentSupportsDockerPercentage(string rawValue, double expectedPercent)
    {
        Assert.That(ContainerStatsParser.ParseCpuPercent(rawValue), Is.EqualTo(expectedPercent));
    }

    [TestCase("")]
    [TestCase("cpu")]
    [TestCase("-1%")]
    public void ParseCpuPercentRejectsInvalidValues(string rawValue)
    {
        Assert.Throws<FormatException>(() => ContainerStatsParser.ParseCpuPercent(rawValue));
    }

    [TestCase("1 KiB", 1024)]
    [TestCase("2.5MiB", 2_621_440)]
    [TestCase("1 GiB", 1_073_741_824)]
    public void ParseMemoryBytesSupportsBinaryUnits(string rawValue, long expectedBytes)
    {
        Assert.That(ContainerStatsParser.ParseMemoryBytes(rawValue), Is.EqualTo(expectedBytes));
    }

    [TestCase("")]
    [TestCase("42")]
    [TestCase("wat MiB")]
    public void ParseMemoryBytesRejectsInvalidValues(string rawValue)
    {
        Assert.Throws<FormatException>(() => ContainerStatsParser.ParseMemoryBytes(rawValue));
    }
}