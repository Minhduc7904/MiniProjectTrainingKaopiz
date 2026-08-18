using System.Text;

namespace BuildingBlocks.Messaging;

/// <summary>Áp convention tên queue RabbitMQ ổn định: <c>service--message-type-kebab-case</c>.</summary>
public static class MessageEndpointNameFormatter
{
    /// <summary>Nhận service owner và command type, trả queue đích của command.</summary>
    public static string ForCommand(string destinationService, Type commandType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationService);
        ArgumentNullException.ThrowIfNull(commandType);

        return $"{ToKebabCase(destinationService)}--{ToKebabCase(commandType.Name)}";
    }

    /// <summary>Nhận subscriber service và event type, trả queue fan-out riêng của subscriber.</summary>
    public static string ForSubscriber(string subscriberService, Type eventType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberService);
        ArgumentNullException.ThrowIfNull(eventType);

        return $"{ToKebabCase(subscriberService)}--{ToKebabCase(eventType.Name)}";
    }

    /// <summary>Chuẩn hóa PascalCase, dấu cách, gạch dưới và dấu chấm thành kebab-case dùng an toàn trong endpoint name.</summary>
    public static string ToKebabCase(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var result = new StringBuilder(value.Length + 8);

        // Tách từ ở ranh giới lower-to-upper và acronym-to-word, đồng thời gộp nhiều separator thành một dấu '-'.
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
