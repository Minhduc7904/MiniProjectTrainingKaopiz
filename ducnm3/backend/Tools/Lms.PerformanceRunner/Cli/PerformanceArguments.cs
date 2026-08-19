using System.Globalization;
using Lms.PerformanceRunner.Configuration;

namespace Lms.PerformanceRunner.Cli;

public static class PerformanceArguments
{
    private static readonly int[] BatchCounts = [3_000, 10_000, 100_000];
    private static readonly int[] CsvCounts = [10_000, 100_000, 300_000];

    public static PerformanceOptions Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || args[0] is "--help" or "-h")
        {
            throw new PerformanceArgumentException("Use batch or csv followed by options. Use --help for usage.");
        }

        var command = args[0] switch
        {
            "batch" => PerformanceCommand.Batch,
            "csv" => PerformanceCommand.Csv,
            _ => throw new PerformanceArgumentException("Command must be either batch or csv."),
        };
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var flags = new HashSet<string>(StringComparer.Ordinal);
        var valueOptions = new HashSet<string>(
            ["--users", "--records", "--approach", "--gateway-url", "--output", "--warmup", "--runs", "--poll-interval-ms", "--memory-sample-interval-ms", "--timeout"],
            StringComparer.Ordinal);
        var flagOptions = new HashSet<string>(
            ["--all", "--prepare-data", "--confirm-reset", "--plain"],
            StringComparer.Ordinal);

        for (var index = 1; index < args.Count; index++)
        {
            var argument = args[index];
            if (flagOptions.Contains(argument))
            {
                flags.Add(argument);
                continue;
            }

            if (!valueOptions.Contains(argument) || ++index >= args.Count)
            {
                throw new PerformanceArgumentException($"Invalid option '{argument}'.");
            }

            values[argument] = args[index];
        }

        var usesAll = flags.Contains("--all");
        var recipientCounts = command == PerformanceCommand.Batch
            ? ResolveCounts(values, "--users", usesAll, BatchCounts, "--users")
            : [];
        var recordCounts = command == PerformanceCommand.Csv
            ? ResolveCounts(values, "--records", usesAll, CsvCounts, "--records")
            : [];
        if (usesAll && (values.ContainsKey("--users") || values.ContainsKey("--records")))
        {
            throw new PerformanceArgumentException("--all cannot be combined with an explicit dataset count.");
        }

        if (command == PerformanceCommand.Batch && values.ContainsKey("--approach"))
        {
            throw new PerformanceArgumentException("--approach is only valid for csv.");
        }

        if (command == PerformanceCommand.Batch && values.ContainsKey("--records"))
        {
            throw new PerformanceArgumentException("--records is only valid for csv.");
        }

        if (command == PerformanceCommand.Csv && values.ContainsKey("--users"))
        {
            throw new PerformanceArgumentException("--users is only valid for batch.");
        }

        var approach = values.TryGetValue("--approach", out var rawApproach)
            ? rawApproach switch
            {
                "buffered" => CsvApproach.Buffered,
                "streaming" => CsvApproach.Streaming,
                "both" => CsvApproach.Both,
                _ => throw new PerformanceArgumentException("--approach must be buffered, streaming, or both."),
            }
            : CsvApproach.Both;
        var warmup = ReadInteger(values, "--warmup", 1, 0, int.MaxValue);
        var runs = ReadInteger(values, "--runs", 3, 1, int.MaxValue);
        var pollInterval = ReadInteger(values, "--poll-interval-ms", 500, 1, int.MaxValue);
        var sampleInterval = ReadInteger(values, "--memory-sample-interval-ms", 1_000, 250, int.MaxValue);
        var timeout = ReadTimeout(values, "--timeout", TimeSpan.FromMinutes(command == PerformanceCommand.Batch ? 30 : 10));
        var gatewayUrl = values.TryGetValue("--gateway-url", out var rawGateway)
            ? CreateGatewayUri(rawGateway)
            : new Uri("http://localhost:5100", UriKind.Absolute);
        var prepareData = flags.Contains("--prepare-data");
        var confirmReset = flags.Contains("--confirm-reset");
        if (prepareData != confirmReset)
        {
            throw new PerformanceArgumentException("--prepare-data requires --confirm-reset, and vice versa.");
        }

        return new PerformanceOptions(
            command,
            recipientCounts,
            recordCounts,
            approach,
            gatewayUrl,
            values.GetValueOrDefault("--output") ?? "performance/results",
            warmup,
            runs,
            pollInterval,
            sampleInterval,
            timeout,
            prepareData,
            confirmReset,
            flags.Contains("--plain"));
    }

    public static string HelpText => """
        Usage:
          Lms.PerformanceRunner batch --users <3000|10000|100000> [options]
          Lms.PerformanceRunner batch --all [options]
          Lms.PerformanceRunner csv --records <10000|100000|300000> [options]
          Lms.PerformanceRunner csv --all [--approach buffered|streaming|both] [options]

        Shared options:
          --gateway-url <url>             Default: http://localhost:5100
          --output <path>                 Default: performance/results
          --warmup <n>                    Default: 1
          --runs <n>                      Default: 3
          --poll-interval-ms <n>          Default: 500
          --memory-sample-interval-ms <n> Default: 1000, minimum: 250
          --timeout <c>                   Example: 00:30:00
          --prepare-data --confirm-reset  Reset and deterministic seed Development databases
          --plain                         Disable live terminal rendering
        """;

    private static IReadOnlyList<int> ResolveCounts(
        Dictionary<string, string> values,
        string option,
        bool usesAll,
        IReadOnlyList<int> allowed,
        string displayOption)
    {
        if (usesAll) return allowed;
        if (!values.TryGetValue(option, out var raw))
        {
            throw new PerformanceArgumentException($"{displayOption} is required unless --all is supplied.");
        }

        if (!int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var count) || !allowed.Contains(count))
        {
            throw new PerformanceArgumentException($"{displayOption} must be one of: {string.Join(", ", allowed)}.");
        }

        return [count];
    }

    private static int ReadInteger(
        Dictionary<string, string> values,
        string option,
        int defaultValue,
        int minimum,
        int maximum)
    {
        if (!values.TryGetValue(option, out var raw)) return defaultValue;
        if (!int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value < minimum || value > maximum)
        {
            throw new PerformanceArgumentException($"{option} must be an integer between {minimum:N0} and {maximum:N0}.");
        }

        return value;
    }

    private static TimeSpan ReadTimeout(Dictionary<string, string> values, string option, TimeSpan defaultValue)
    {
        if (!values.TryGetValue(option, out var raw)) return defaultValue;
        if (!TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out var value) || value <= TimeSpan.Zero)
        {
            throw new PerformanceArgumentException($"{option} must be a positive TimeSpan.");
        }

        return value;
    }

    private static Uri CreateGatewayUri(string raw)
    {
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
        {
            throw new PerformanceArgumentException("--gateway-url must be an absolute HTTP(S) URL.");
        }

        return uri;
    }
}
