using System.Globalization;

namespace Lms.PerformanceRunner.Monitoring;

public static class ContainerStatsParser
{
    public static double ParseCpuPercent(string rawValue)
    {
        var value = rawValue.Trim().TrimEnd('%');
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent)
            || percent < 0)
        {
            throw new FormatException($"Docker stats CPU value '{rawValue}' is invalid.");
        }

        return percent;
    }

    public static long ParseMemoryBytes(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw new FormatException("Docker stats memory value is empty.");
        }

        var value = rawValue.Trim();
        var unitStart = value.IndexOfAny(['k', 'K', 'm', 'M', 'g', 'G', 't', 'T']);
        if (unitStart < 0)
        {
            throw new FormatException($"Docker stats memory value '{rawValue}' has no supported unit.");
        }

        var number = value[..unitStart].Trim();
        var unit = char.ToUpperInvariant(value[unitStart]);
        if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var amount) || amount < 0)
        {
            throw new FormatException($"Docker stats memory value '{rawValue}' is invalid.");
        }

        var multiplier = unit switch
        {
            'K' => 1024d,
            'M' => 1024d * 1024d,
            'G' => 1024d * 1024d * 1024d,
            'T' => 1024d * 1024d * 1024d * 1024d,
            _ => throw new FormatException($"Docker stats memory unit '{unit}' is unsupported."),
        };

        var bytes = amount * multiplier;
        if (bytes > long.MaxValue)
        {
            throw new FormatException("Docker stats memory value is too large.");
        }

        return checked((long)bytes);
    }
}