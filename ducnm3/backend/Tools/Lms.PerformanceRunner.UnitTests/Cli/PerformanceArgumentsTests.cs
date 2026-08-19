using Lms.PerformanceRunner.Cli;
using Lms.PerformanceRunner.Configuration;

namespace Lms.PerformanceRunner.UnitTests.Cli;

public sealed class PerformanceArgumentsTests
{
    [Test]
    public void ParseBatchWithPrepareDataWithoutConfirmationRejectsMutation()
    {
        var exception = Assert.Throws<PerformanceArgumentException>(() =>
            PerformanceArguments.Parse(["batch", "--users", "3000", "--prepare-data"]));

        Assert.That(exception!.Message, Does.Contain("--confirm-reset"));
    }

    [Test]
    public void ParseCsvAtMaximumRecordCountAcceptsBothApproaches()
    {
        var options = PerformanceArguments.Parse(
            ["csv", "--records", "300000", "--approach", "both", "--plain"]);

        Assert.Multiple(() =>
        {
            Assert.That(options.Command, Is.EqualTo(PerformanceCommand.Csv));
            Assert.That(options.RecordCounts, Is.EqualTo([300_000]));
            Assert.That(options.Approach, Is.EqualTo(CsvApproach.Both));
            Assert.That(options.PlainOutput, Is.True);
        });
    }

    [TestCase("batch", "--users", "2999")]
    [TestCase("batch", "--users", "100001")]
    [TestCase("csv", "--records", "9999")]
    [TestCase("csv", "--records", "300001")]
    public void ParseOutOfRangeDatasetCountRejects(string command, string option, string value)
    {
        Assert.Throws<PerformanceArgumentException>(() =>
            PerformanceArguments.Parse([command, option, value]));
    }

    [Test]
    public void ParseSamplingIntervalBelowMinimumRejects()
    {
        Assert.Throws<PerformanceArgumentException>(() =>
            PerformanceArguments.Parse(
                ["batch", "--users", "3000", "--memory-sample-interval-ms", "249"]));
    }

    [Test]
    public void ParseRejectsOptionsBelongingToAnotherCommand()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<PerformanceArgumentException>(() =>
                PerformanceArguments.Parse(["batch", "--users", "3000", "--approach", "both"]));
            Assert.Throws<PerformanceArgumentException>(() =>
                PerformanceArguments.Parse(["csv", "--records", "10000", "--users", "3000"]));
        });
    }
}
