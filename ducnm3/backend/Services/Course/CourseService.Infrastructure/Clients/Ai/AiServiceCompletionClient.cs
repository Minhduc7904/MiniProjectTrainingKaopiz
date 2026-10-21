// File: backend/Services/Course/CourseService.Infrastructure/Clients/Ai/AiServiceCompletionClient.cs
// Mục đích: Gọi POST /api/ai/completions của AI Service bằng actor ADMIN và đổi response/lỗi thành kiểu của Course Application.

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Contracts.Api;
using CourseService.Application.Services.Ai;

namespace CourseService.Infrastructure.Clients.Ai;

public sealed class AiServiceCompletionClient(HttpClient httpClient) : IAiCompletionClient
{
    public const string UnavailableCode = "AI_SERVICE_UNAVAILABLE";
    public const string TimeoutCode = "AI_SERVICE_TIMEOUT";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<AiCompletion> CompleteAsync(
        string task,
        IReadOnlyList<AiMessage> messages,
        IReadOnlyList<AiToolDefinition> tools,
        Guid actorId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Ai.CompletionsServicePath())
        {
            Content = JsonContent.Create(new CompletionRequest(task, messages, tools), options: JsonOptions),
        };
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, actorId.ToString("D"));

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiCompletionException(TimeoutCode, "AI Service did not respond in time.");
        }
        catch (HttpRequestException)
        {
            throw new AiCompletionException(UnavailableCode, "AI Service is unavailable.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode) throw await ReadErrorAsync(response, cancellationToken);

            var envelope = await ReadAsync<ApiResponse<CompletionPayload>>(response, cancellationToken);
            var data = envelope?.Data ?? throw new AiCompletionException(UnavailableCode, "AI Service returned an unreadable response.");
            return new AiCompletion(
                data.Text,
                (data.ToolCalls ?? []).Select(call => new AiToolCall(call.Id, call.Name, call.ArgumentsJson)).ToArray(),
                data.FinishReason ?? string.Empty,
                data.PromptTokens,
                data.CompletionTokens);
        }
    }

    private static async Task<AiCompletionException> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var envelope = await ReadAsync<ApiErrorResponse>(response, cancellationToken);
        return envelope?.Error is { } error && !string.IsNullOrWhiteSpace(error.Code)
            ? new AiCompletionException(error.Code, error.Message)
            : new AiCompletionException(UnavailableCode, $"AI Service returned HTTP {(int)response.StatusCode}.");
    }

    private static async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private sealed record CompletionRequest(string Task, IReadOnlyList<AiMessage> Messages, IReadOnlyList<AiToolDefinition> Tools);

    private sealed record CompletionPayload(
        string? Text,
        IReadOnlyList<AiToolCall>? ToolCalls,
        string? FinishReason,
        int PromptTokens,
        int CompletionTokens);
}
