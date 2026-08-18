// File: backend/Services/Media/MediaService.Infrastructure/Services/Thumbnail/NativeProcessRunner.cs
// Mục đích: Chạy process native như FFmpeg với timeout và thu thập lỗi, được MediaThumbnailGenerator dùng để trích thumbnail video.

using System.Diagnostics;

namespace MediaService.Infrastructure.Services.Thumbnail;

internal sealed class NativeProcessRunner(TimeSpan timeout)
{
    public async Task<string> RunAsync(
        string executable,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ??
            throw new InvalidOperationException(
                $"Unable to start required media tool '{executable}'.");
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        timeoutSource.CancelAfter(timeout);
        var outputTask = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
        var errorTask = process.StandardError.ReadToEndAsync(timeoutSource.Token);

        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
            var output = await outputTask;
            var error = await errorTask;
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Media tool '{executable}' failed with exit code {process.ExitCode}: " +
                    Truncate(error));
            }

            return output;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            throw new TimeoutException(
                $"Media tool '{executable}' exceeded the configured timeout.");
        }
        catch
        {
            TryKill(process);
            throw;
        }
    }

    private static string Truncate(string value) =>
        value.Length <= 500 ? value : value[..500];

    private static void TryKill(Process process)
    {
        if (process.HasExited)
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }
    }
}
