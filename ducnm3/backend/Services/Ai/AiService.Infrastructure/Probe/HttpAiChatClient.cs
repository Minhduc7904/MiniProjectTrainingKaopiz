// File: backend/Services/Ai/AiService.Infrastructure/Probe/HttpAiChatClient.cs
// Mục đích: Gọi POST {baseUrl}/chat/completions (chuẩn OpenAI-compatible) kèm tools, ảnh và tệp PDF; làm sạch mọi lỗi trước khi trả ra ngoài.

using System.Net;
using System.Text.Json;
using AiService.Application.Services;
using AiService.Domain.Constants;

namespace AiService.Infrastructure.Probe;

public sealed class HttpAiChatClient(HttpClient httpClient) : IAiChatClient
{
    /// <summary>Một lượt gọi model có thể đọc tài liệu dài nên timeout lớn hơn nhiều so với health check.</summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(240);

    private const string ChatPath = "chat/completions";
    private const string AnthropicVersion = "2023-06-01";
    private const int MaxProviderMessageLength = 300;

    public async Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.BaseUrl.TrimEnd('/') + "/" + ChatPath, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new AiChatException(AiChatFailureKind.Unreachable, null, "Base URL is not a valid http(s) address.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new ByteArrayContent(BuildPayload(request)),
        };
        message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        message.Headers.TryAddWithoutValidation("Authorization", $"Bearer {request.ApiKey}");
        if (request.ProviderType == AiProviderTypes.Anthropic)
        {
            message.Headers.TryAddWithoutValidation("x-api-key", request.ApiKey);
            message.Headers.TryAddWithoutValidation("anthropic-version", AnthropicVersion);
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(Timeout);
            using var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead, timeout.Token);
            var body = await response.Content.ReadAsStringAsync(timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                throw MapFailure(response.StatusCode, body, request.ApiKey);
            }

            return ParseResponse(body);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiChatException(AiChatFailureKind.Timeout, null, "Provider did not respond in time.");
        }
        catch (HttpRequestException)
        {
            // Không đưa message của exception ra ngoài: có thể chứa host nội bộ.
            throw new AiChatException(AiChatFailureKind.Unreachable, null, "Cannot connect to the provider base URL.");
        }
    }

    internal static byte[] BuildPayload(AiChatRequest request)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", request.ModelName);
            writer.WritePropertyName("messages");
            writer.WriteStartArray();
            foreach (var message in request.Messages) WriteMessage(writer, message, request.ProviderType);
            writer.WriteEndArray();
            if (request.Tools.Count > 0)
            {
                writer.WritePropertyName("tools");
                writer.WriteStartArray();
                foreach (var tool in request.Tools)
                {
                    writer.WriteStartObject();
                    writer.WriteString("type", "function");
                    writer.WritePropertyName("function");
                    writer.WriteStartObject();
                    writer.WriteString("name", tool.Name);
                    writer.WriteString("description", tool.Description);
                    writer.WritePropertyName("parameters");
                    tool.ParametersSchema.WriteTo(writer);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    private static void WriteMessage(Utf8JsonWriter writer, AiChatMessage message, string providerType)
    {
        writer.WriteStartObject();
        writer.WriteString("role", message.Role);
        if (message.Role == "tool")
        {
            writer.WriteString("tool_call_id", message.ToolCallId);
        }

        if (message.Parts is { Count: > 0 })
        {
            writer.WritePropertyName("content");
            writer.WriteStartArray();
            foreach (var part in message.Parts) WritePart(writer, part, providerType);
            writer.WriteEndArray();
        }
        else if (message.Text is not null)
        {
            writer.WriteString("content", message.Text);
        }
        else
        {
            writer.WriteNull("content");
        }

        if (message.ToolCalls is { Count: > 0 })
        {
            writer.WritePropertyName("tool_calls");
            writer.WriteStartArray();
            foreach (var call in message.ToolCalls)
            {
                writer.WriteStartObject();
                writer.WriteString("id", call.Id);
                writer.WriteString("type", "function");
                writer.WritePropertyName("function");
                writer.WriteStartObject();
                writer.WriteString("name", call.Name);
                writer.WriteString("arguments", call.ArgumentsJson);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    private static void WritePart(Utf8JsonWriter writer, AiChatContentPart part, string providerType)
    {
        writer.WriteStartObject();
        switch (part.Type)
        {
            case "text":
                writer.WriteString("type", "text");
                writer.WriteString("text", part.Text);
                break;
            case "image":
                WriteDataUriPart(writer, part);
                break;
            // Lớp tương thích OpenAI của Gemini từ chối part "file" (HTTP 400 "Invalid content part type: file")
            // nhưng nhận PDF dưới dạng data URI trong "image_url", nên chuyển PDF sang dạng đó cho Gemini.
            case "file" when providerType == AiProviderTypes.Gemini:
                WriteDataUriPart(writer, part);
                break;
            default:
                writer.WriteString("type", "file");
                writer.WritePropertyName("file");
                writer.WriteStartObject();
                writer.WriteString("filename", string.IsNullOrWhiteSpace(part.FileName) ? "document.pdf" : part.FileName);
                writer.WriteString("file_data", $"data:{part.MediaType};base64,{part.DataBase64}");
                writer.WriteEndObject();
                break;
        }

        writer.WriteEndObject();
    }

    private static void WriteDataUriPart(Utf8JsonWriter writer, AiChatContentPart part)
    {
        writer.WriteString("type", "image_url");
        writer.WritePropertyName("image_url");
        writer.WriteStartObject();
        writer.WriteString("url", $"data:{part.MediaType};base64,{part.DataBase64}");
        writer.WriteEndObject();
    }

    internal static AiChatResult ParseResponse(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var choice = document.RootElement.GetProperty("choices")[0];
            var message = choice.GetProperty("message");
            var text = message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String
                ? content.GetString()
                : null;

            var toolCalls = new List<AiChatToolCall>();
            if (message.TryGetProperty("tool_calls", out var calls) && calls.ValueKind == JsonValueKind.Array)
            {
                foreach (var call in calls.EnumerateArray())
                {
                    var function = call.GetProperty("function");
                    var id = call.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
                    var arguments = function.TryGetProperty("arguments", out var args)
                        ? args.ValueKind == JsonValueKind.String ? args.GetString() : args.GetRawText()
                        : null;
                    // Một số provider trả id rỗng; tạo id cục bộ để message tool trả lời vẫn ghép được cặp.
                    toolCalls.Add(new AiChatToolCall(
                        string.IsNullOrEmpty(id) ? $"call_{toolCalls.Count + 1}" : id,
                        function.GetProperty("name").GetString() ?? string.Empty,
                        string.IsNullOrWhiteSpace(arguments) ? "{}" : arguments));
                }
            }

            var finishReason = choice.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String
                ? finish.GetString() ?? string.Empty
                : string.Empty;
            var promptTokens = 0;
            var completionTokens = 0;
            if (document.RootElement.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                promptTokens = ReadInt(usage, "prompt_tokens");
                completionTokens = ReadInt(usage, "completion_tokens");
            }

            return new AiChatResult(text, toolCalls, finishReason, promptTokens, completionTokens);
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException or IndexOutOfRangeException)
        {
            throw new AiChatException(AiChatFailureKind.InvalidResponse, null, "Provider returned an unreadable response.");
        }
    }

    private static int ReadInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : 0;

    private static AiChatException MapFailure(HttpStatusCode statusCode, string body, string apiKey)
    {
        var status = (int)statusCode;
        return statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                new AiChatException(AiChatFailureKind.AuthFailed, status, "Provider rejected the API key."),
            HttpStatusCode.TooManyRequests =>
                new AiChatException(AiChatFailureKind.RateLimited, status, "Provider rate limit was reached."),
            HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.UnprocessableEntity or HttpStatusCode.RequestEntityTooLarge =>
                new AiChatException(AiChatFailureKind.Rejected, status, RejectedMessage(status, body, apiKey)),
            _ => new AiChatException(AiChatFailureKind.InvalidResponse, status, $"Provider returned HTTP {status}."),
        };
    }

    // Lời nhắn lỗi 4xx của provider thường chỉ rõ vì sao (model không hỗ trợ tool, tệp quá lớn...), nên giữ lại
    // sau khi cắt ngắn và che API key nếu provider lỡ lặp lại.
    private static string RejectedMessage(int status, string body, string apiKey)
    {
        var detail = ExtractProviderMessage(body);
        if (string.IsNullOrWhiteSpace(detail)) return $"Provider rejected the request (HTTP {status}).";
        if (apiKey.Length > 0) detail = detail.Replace(apiKey, "***", StringComparison.Ordinal);
        if (detail.Length > MaxProviderMessageLength) detail = detail[..MaxProviderMessageLength] + "...";
        return $"Provider rejected the request (HTTP {status}): {detail}";
    }

    private static string? ExtractProviderMessage(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0) root = root[0];
            if (root.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.String) return error.GetString();
                if (error.ValueKind == JsonValueKind.Object && error.TryGetProperty("message", out var message))
                {
                    return message.GetString();
                }
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }
}
