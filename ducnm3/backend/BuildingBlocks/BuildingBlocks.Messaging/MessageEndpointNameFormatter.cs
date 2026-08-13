using System.Text;

namespace BuildingBlocks.Messaging;

public static class MessageEndpointNameFormatter
{
    public static string ForCommand(string destinationService, Type commandType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationService);
        ArgumentNullException.ThrowIfNull(commandType);

        return $"{ToKebabCase(destinationService)}--{ToKebabCase(commandType.Name)}";
    }

    public static string ForSubscriber(string subscriberService, Type eventType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberService);
        ArgumentNullException.ThrowIfNull(eventType);

        return $"{ToKebabCase(subscriberService)}--{ToKebabCase(eventType.Name)}";
    }

    public static string ToKebabCase(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var result = new StringBuilder(value.Length + 8);

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character is '_' or ' ' or '.')
            {
                AppendSeparator(result);
                continue;
            }

            if (char.IsUpper(character) &&
                index > 0 &&
                result.Length > 0 &&
                result[^1] != '-' &&
                (char.IsLower(value[index - 1]) ||
                 (index + 1 < value.Length && char.IsLower(value[index + 1]))))
            {
                result.Append('-');
            }

            result.Append(char.ToLowerInvariant(character));
        }

        return result.ToString().Trim('-');
    }

    private static void AppendSeparator(StringBuilder result)
    {
        if (result.Length > 0 && result[^1] != '-')
        {
            result.Append('-');
        }
    }
}
