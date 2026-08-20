using System.Diagnostics;

namespace Lms.PerformanceRunner.Monitoring;

public sealed class ContainerStatsMonitor(
    string composeProjectDirectory,
    string service,
    TimeSpan sampleInterval,
    int baselineSampleCount = 5)
{
    public async Task<ContainerStatsCapture> CaptureAsync(
        Func<CancellationToken, Task> measurement,
        CancellationToken cancellationToken,
        IProgress<ContainerStatsSample>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(measurement);
        ArgumentOutOfRangeException.ThrowIfLessThan(
            sampleInterval,
            TimeSpan.FromMilliseconds(250));

        var containerId = await ResolveContainerIdAsync(cancellationToken);
        var samples = new List<ContainerStatsSample>();
        var stopwatch = Stopwatch.StartNew();

        for (var index = 0; index < baselineSampleCount; index++)
        {
            var sample = await ReadSampleAsync(containerId, stopwatch.Elapsed, cancellationToken);
            samples.Add(sample);
            progress?.Report(sample);
            if (index + 1 < baselineSampleCount)
            {
                await Task.Delay(sampleInterval, cancellationToken);
            }
        }

        using var samplingCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var samplingTask = SampleUntilCancelledAsync(
            containerId,
            stopwatch,
            samples,
            samplingCancellation.Token,
            progress);
        try
        {
            await measurement(cancellationToken);
        }
        finally
        {
            await samplingCancellation.CancelAsync();
            try
            {
                await samplingTask;
            }
            catch (OperationCanceledException) when (samplingCancellation.IsCancellationRequested)
            {
            }

            try
            {
                samples.Add(await ReadSampleAsync(containerId, stopwatch.Elapsed, CancellationToken.None));
            }
            catch when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        if (samples.Count == 0)
        {
            throw new InvalidOperationException("Container stats produced no samples.");
        }

        return new ContainerStatsCapture(
            containerId,
            samples,
            MemoryStatistics.Calculate(samples, Math.Min(baselineSampleCount, samples.Count)),
            ResourceStatistics.Calculate(samples));
    }

    private async Task SampleUntilCancelledAsync(
        string containerId,
        Stopwatch stopwatch,
        List<ContainerStatsSample> samples,
        CancellationToken cancellationToken,
        IProgress<ContainerStatsSample>? progress)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(sampleInterval, cancellationToken);
            if (cancellationToken.IsCancellationRequested) break;
            var sample = await ReadSampleAsync(containerId, stopwatch.Elapsed, cancellationToken);
            samples.Add(sample);
            progress?.Report(sample);
        }
    }

    private async Task<string> ResolveContainerIdAsync(CancellationToken cancellationToken)
    {
        var result = await RunDockerAsync(
            ["compose", "ps", "-q", service],
            cancellationToken);
        var containerId = result.StandardOutput.Trim();
        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(containerId))
        {
            throw new InvalidOperationException(
                $"Unable to resolve Docker Compose container for service '{service}'.");
        }

        return containerId.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
    }

    private async Task<ContainerStatsSample> ReadSampleAsync(
        string containerId,
        TimeSpan elapsed,
        CancellationToken cancellationToken)
    {
        var result = await RunDockerAsync(
            ["stats", "--no-stream", "--format", "{{.CPUPerc}}\\t{{.MemUsage}}", containerId],
            cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException("docker stats failed.");
        }

        var fields = result.StandardOutput.Trim().Split('\t', 2);
        if (fields.Length != 2)
        {
            throw new FormatException("Docker stats did not return CPU and memory fields.");
        }

        var memory = fields[1].Split('/', 2)[0].Trim();
        return new ContainerStatsSample(
            DateTimeOffset.UtcNow,
            elapsed,
            service,
            containerId.Length > 12 ? containerId[..12] : containerId,
            ContainerStatsParser.ParseMemoryBytes(memory),
            ContainerStatsParser.ParseCpuPercent(fields[0]));
    }

    private Task<DockerCommandResult> RunDockerAsync(
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            WorkingDirectory = composeProjectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);

        return RunProcessAsync(startInfo, cancellationToken);
    }

    private static async Task<DockerCommandResult> RunProcessAsync(
        ProcessStartInfo startInfo,
        CancellationToken cancellationToken)
    {
        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        if (!process.Start()) throw new InvalidOperationException("Unable to start docker process.");
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return new DockerCommandResult(process.ExitCode, await outputTask, await errorTask);
    }

    private sealed record DockerCommandResult(int ExitCode, string StandardOutput, string StandardError);
}

public sealed record ContainerStatsCapture(
    string ContainerId,
    IReadOnlyList<ContainerStatsSample> Samples,
    MemoryStatistics Memory,
    ResourceStatistics Resources);