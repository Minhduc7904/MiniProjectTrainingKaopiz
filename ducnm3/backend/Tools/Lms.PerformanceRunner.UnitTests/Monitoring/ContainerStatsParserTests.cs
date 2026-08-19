using Lms.PerformanceRunner.Monitoring;

namespace Lms.PerformanceRunner.UnitTests.Monitoring;

public sealed class ContainerStatsParserTests
{
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