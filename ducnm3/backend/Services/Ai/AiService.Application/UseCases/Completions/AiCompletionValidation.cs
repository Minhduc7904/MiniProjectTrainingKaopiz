// File: backend/Services/Ai/AiService.Application/UseCases/Completions/AiCompletionValidation.cs
// Mục đích: Validate request completion (task, message, tool, kích thước nội dung) trước khi gọi provider.

using System.Text.RegularExpressions;
using AiService.Application.Common;
using AiService.Application.Services;
using AiService.Application.UseCases.Bindings;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Completions;

internal static partial class AiCompletionValidation
{
    public const int MaxMessages = 200;
    public const int MaxTools = 16;
    public const int MaxPartsPerMessage = 64;

    /// <summary>Tổng ký tự văn bản + base64 của một request; chặn payload quá lớn trước khi gửi ra ngoài.</summary>
    public const long MaxPayloadCharacters = 24_000_000;

    private static readonly string[] Roles = ["system", "user", "assistant", "tool"];
    private static readonly string[] PartTypes = ["text", "image", "file"];

    public static string NormalizeTask(string? task, List<ApiErrorDetail> details)
    {
        var normalized = AiServiceNames.Normalize(task);
        if (!AiBindableServices.IsSupported(normalized))
        {
            details.Add(new ApiErrorDetail("task", $"Task must be one of {string.Join(", ", AiBindableServices.All)}."));
        }

        return normalized;
    }

    public static void ValidateMessages(IReadOnlyList<AiChatMessage>? messages, List<ApiErrorDetail> details)
    {
        if (messages is null || messages.Count == 0)
        {
            details.Add(new ApiErrorDetail("messages", "At least one message is required."));
            return;
        }

        if (messages.Count > MaxMessages)
        {
            details.Add(new ApiErrorDetail("messages", $"At most {MaxMessages} messages are allowed."));
            return;
        }

        long characters = 0;
        for (var index = 0; index < messages.Count; index++)
        {
            var message = messages[index];
            var field = $"messages[{index}]";
            if (message is null || !Roles.Contains(message.Role, StringComparer.Ordinal))
            {
                details.Add(new ApiErrorDetail($"{field}.role", $"Role must be one of {string.Join(", ", Roles)}."));
                continue;
            }

            if (message.Text is not null && message.Parts is not null)
            {
                details.Add(new ApiErrorDetail(field, "Use either text or parts, not both."));
            }

            var hasToolCalls = message.ToolCalls is { Count: > 0 };
            if (message.Text is null && message.Parts is not { Count: > 0 } && !hasToolCalls)
            {
                details.Add(new ApiErrorDetail(field, "Message needs text, parts or toolCalls."));
            }

            if (hasToolCalls && message.Role != "assistant")
            {
                details.Add(new ApiErrorDetail($"{field}.toolCalls", "Only assistant messages can carry toolCalls."));
            }

            if (message.Role == "tool" && string.IsNullOrWhiteSpace(message.ToolCallId))
            {
                details.Add(new ApiErrorDetail($"{field}.toolCallId", "toolCallId is required for tool messages."));
            }

            characters += message.Text?.Length ?? 0;
            characters += ValidateParts(message.Parts, field, details);
            foreach (var call in message.ToolCalls ?? [])
            {
                characters += call.ArgumentsJson?.Length ?? 0;
            }
        }

        if (characters > MaxPayloadCharacters)
        {
            details.Add(new ApiErrorDetail("messages", "Message content is too large."));
        }
    }

    public static void ValidateTools(IReadOnlyList<AiChatTool>? tools, List<ApiErrorDetail> details)
    {
        if (tools is null) return;
        if (tools.Count > MaxTools)
        {
            details.Add(new ApiErrorDetail("tools", $"At most {MaxTools} tools are allowed."));
            return;
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < tools.Count; index++)
        {
            var tool = tools[index];
            var field = $"tools[{index}]";
            if (tool is null || string.IsNullOrEmpty(tool.Name) || !ToolNamePattern().IsMatch(tool.Name))
            {
                details.Add(new ApiErrorDetail($"{field}.name", "Tool name must be 1-64 characters of letters, digits, '_' or '-'."));
                continue;
            }

            if (!names.Add(tool.Name))
            {
                details.Add(new ApiErrorDetail($"{field}.name", "Tool name must be unique."));
            }

            if (string.IsNullOrWhiteSpace(tool.Description))
            {
                details.Add(new ApiErrorDetail($"{field}.description", "Tool description is required."));
            }

            if (tool.ParametersSchema.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                details.Add(new ApiErrorDetail($"{field}.parametersSchema", "parametersSchema must be a JSON Schema object."));
            }
        }
    }

    private static long ValidateParts(IReadOnlyList<AiChatContentPart>? parts, string field, List<ApiErrorDetail> details)
    {
        if (parts is null) return 0;
        if (parts.Count > MaxPartsPerMessage)
        {
            details.Add(new ApiErrorDetail($"{field}.parts", $"At most {MaxPartsPerMessage} parts are allowed per message."));
            return 0;
        }

        long characters = 0;
        for (var index = 0; index < parts.Count; index++)
        {
            var part = parts[index];
            var partField = $"{field}.parts[{index}]";
            if (part is null || !PartTypes.Contains(part.Type, StringComparer.Ordinal))
            {
                details.Add(new ApiErrorDetail($"{partField}.type", $"Part type must be one of {string.Join(", ", PartTypes)}."));
                continue;
            }

            if (part.Type == "text")
            {
                if (part.Text is null) details.Add(new ApiErrorDetail($"{partField}.text", "text is required for a text part."));
                characters += part.Text?.Length ?? 0;
                continue;
            }

            if (string.IsNullOrWhiteSpace(part.DataBase64) || string.IsNullOrWhiteSpace(part.MediaType))
            {
                details.Add(new ApiErrorDetail(partField, "mediaType and dataBase64 are required for image and file parts."));
                continue;
            }

            if (part.Type == "image" && !part.MediaType.StartsWith("image/", StringComparison.Ordinal))
            {
                details.Add(new ApiErrorDetail($"{partField}.mediaType", "Image parts need an image/* media type."));
            }

            characters += part.DataBase64.Length;
        }

        return characters;
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{1,64}$")]
    private static partial Regex ToolNamePattern();
}
