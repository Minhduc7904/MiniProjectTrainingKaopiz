// File: backend/Services/Ai/AiService.Application/UseCases/Providers/HealthCheck/CheckAiProviderHealthHandler.cs
// Mục đích: Thử gọi một API nhẹ của từng AI provider bằng API key đã lưu để báo trạng thái kết nối; không lưu kết quả.

using System.Security.Cryptography;
using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Application.Services;
using AiService.Domain.Constants;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Providers.HealthCheck;

public sealed class CheckAiProviderHealthHandler(
    IAiProviderRepository repository,
    ISecretProtector secretProtector,
    IAiProviderProbe probe)
{
    // Giới hạn số cuộc gọi ra ngoài chạy song song để một lần mở trang không bắn hàng loạt request cùng lúc.
    private const int MaxConcurrentProbes = 8;

    public async Task<AiProviderHealthResult> HandleAsync(Guid providerId, CancellationToken cancellationToken)
    {
        if (providerId == Guid.Empty)
        {
            throw AiErrors.ValidationFailed([new ApiErrorDetail("providerId", "providerId must be a valid UUID.")]);
        }

        var credential = await repository.GetCredentialAsync(providerId, cancellationToken)
            ?? throw AiErrors.ProviderNotFound();
        return await CheckAsync(credential, cancellationToken);
    }

    public async Task<IReadOnlyList<AiProviderHealthResult>> HandleAllAsync(CancellationToken cancellationToken)
    {
        var credentials = await repository.GetAllCredentialsAsync(cancellationToken);
        using var gate = new SemaphoreSlim(MaxConcurrentProbes);
        var tasks = credentials.Select(async credential =>
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                return await CheckAsync(credential, cancellationToken);
            }
            finally
            {
                gate.Release();
            }
        });
        return await Task.WhenAll(tasks);
    }

    private async Task<AiProviderHealthResult> CheckAsync(AiProviderCredentialRecord credential, CancellationToken cancellationToken)
    {
        var provider = credential.Provider;
        if (provider.Status != AiStatuses.Active)
        {
            return Result(provider, AiProviderHealthStatuses.Disabled, null, null, "Provider is inactive, connection was not tested.");
        }

        string apiKey;
        try
        {
            apiKey = secretProtector.Unprotect(credential.ApiKeyEncrypted);
        }
        catch (Exception exception) when (exception is CryptographicException or FormatException)
        {
            return Result(provider, AiProviderHealthStatuses.Error, null, null, "Stored API key cannot be decrypted. Enter the key again.");
        }

        var probed = await probe.ProbeAsync(provider.ProviderType, provider.BaseUrl, apiKey, cancellationToken);
        return Result(provider, probed.Status, probed.HttpStatus, probed.LatencyMs, probed.Message);
    }

    private static AiProviderHealthResult Result(AiProviderRecord provider, string status, int? httpStatus, long? latencyMs, string message) =>
        new(provider, status, httpStatus, latencyMs, message, DateTime.UtcNow);
}
