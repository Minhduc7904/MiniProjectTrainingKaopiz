// File: backend/Services/Ai/AiService.Application/UseCases/Completions/Create/CreateAiCompletionHandler.cs
// Mục đích: Chạy một lượt chat completion cho một tác vụ AI: tra binding, giải mã API key, gọi provider và ghi nhật ký.

using System.Diagnostics;
using System.Security.Cryptography;
using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Application.Services;
using AiService.Application.UseCases.Bindings;
using AiService.Domain.Constants;
using BuildingBlocks.Contracts.Api;
using Microsoft.Extensions.Logging;

namespace AiService.Application.UseCases.Completions.Create;

public sealed class CreateAiCompletionHandler(
    IAiServiceBindingRepository bindingRepository,
    IAiProviderRepository providerRepository,
    ISecretProtector secretProtector,
    IAiChatClient chatClient,
    IAiInvocationRepository invocationRepository,
    ILogger<CreateAiCompletionHandler> logger)
{
    private static readonly Action<ILogger, string, Exception?> LogRecordFailed = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(2001, "AiInvocationRecordFailed"),
        "Could not record AI invocation for task {Task}.");

    private static readonly Action<ILogger, string, string, string, int?, string, Exception?> LogProviderFailed = LoggerMessage.Define<string, string, string, int?, string>(
        LogLevel.Warning,
        new EventId(2002, "AiProviderCallFailed"),
        "AI provider call failed for task {Task} on model {Model}: {Kind} (HTTP {HttpStatus}) {Detail}");

    public async Task<CreateAiCompletionResult> HandleAsync(
        CreateAiCompletionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var details = new List<ApiErrorDetail>();
        var task = AiCompletionValidation.NormalizeTask(command.Task, details);
        AiCompletionValidation.ValidateMessages(command.Messages, details);
        AiCompletionValidation.ValidateTools(command.Tools, details);
        if (details.Count > 0) throw AiErrors.ValidationFailed(details);

        var binding = await bindingRepository.GetByServiceNameAsync(task, cancellationToken)
            ?? throw AiErrors.TaskNotConfigured(task);
        if (binding.ModelStatus != AiStatuses.Active || binding.ProviderStatus != AiStatuses.Active)
        {
            throw AiErrors.ReferenceInactive();
        }

        var credential = await providerRepository.GetCredentialAsync(binding.ProviderId, cancellationToken)
            ?? throw AiErrors.ProviderNotFound();
        string apiKey;
        try
        {
            apiKey = secretProtector.Unprotect(credential.ApiKeyEncrypted);
        }
        catch (Exception exception) when (exception is CryptographicException or FormatException)
        {
            throw AiErrors.ProviderKeyUnreadable();
        }

        var request = new AiChatRequest(
            binding.ProviderType,
            credential.Provider.BaseUrl,
            apiKey,
            binding.ModelName,
            command.Messages!,
            command.Tools ?? []);

        var stopwatch = Stopwatch.StartNew();
        AiChatResult result;
        try
        {
            result = await chatClient.CompleteAsync(request, cancellationToken);
        }
        catch (AiChatException exception)
        {
            // Message của AiChatException đã được làm sạch; ghi lại để biết lý do thật khi API trả mã lỗi chung.
            LogProviderFailed(logger, task, binding.ModelName, exception.Kind.ToString(), exception.HttpStatus, exception.Message, null);
            var error = AiErrors.FromChatFailure(exception);
            await RecordAsync(binding, task, AiInvocationStatuses.Failed, error.ErrorCode, 0, 0, 0, stopwatch.ElapsedMilliseconds);
            throw error;
        }

        var latencyMs = stopwatch.ElapsedMilliseconds;
        await RecordAsync(binding, task, AiInvocationStatuses.Success, null, result.PromptTokens, result.CompletionTokens, result.ToolCalls.Count, latencyMs);
        return new CreateAiCompletionResult(
            task,
            binding.ProviderType,
            binding.ModelName,
            result.Text,
            result.ToolCalls,
            result.FinishReason,
            result.PromptTokens,
            result.CompletionTokens,
            latencyMs);
    }

    // Nhật ký chỉ phục vụ thống kê: lỗi ghi nhật ký không được làm mất kết quả model đã trả tiền.
    private async Task RecordAsync(
        AiServiceBindingRecord binding,
        string task,
        string status,
        string? errorCode,
        int promptTokens,
        int completionTokens,
        int toolCallCount,
        long latencyMs)
    {
        try
        {
            await invocationRepository.AddAsync(
                new AiInvocationRecord(
                    Guid.NewGuid(),
                    task,
                    binding.ModelId,
                    binding.ProviderType,
                    binding.ModelName,
                    status,
                    errorCode,
                    promptTokens,
                    completionTokens,
                    toolCallCount,
                    latencyMs,
                    DateTime.UtcNow),
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            LogRecordFailed(logger, task, exception);
        }
    }
}
