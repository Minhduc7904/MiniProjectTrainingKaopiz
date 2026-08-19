using Lms.PerformanceRunner.Cli;
using Lms.PerformanceRunner.Configuration;

namespace Lms.PerformanceRunner;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Contains("--help", StringComparer.Ordinal) || args.Contains("-h", StringComparer.Ordinal))
        {
            Console.WriteLine(PerformanceArguments.HelpText);
            return 0;
        }

        try
        {
            var options = PerformanceArguments.Parse(args);
            using var cancellationSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationSource.Cancel();
            };
            await BenchmarkApplication.RunAsync(options, cancellationSource.Token);
            return 0;
        }
        catch (PerformanceArgumentException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 2;
        }
        catch (OperationCanceledException)
        {
            return 130;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Benchmark failed: {exception.Message}");
            return 1;
        }
    }
}
