// File: backend/Services/Ai/AiService.Api/Mappers/AiResponseMapper.cs
// Mục đích: Chuyển record của use case AI thành HTTP contract công khai.

using AiService.Api.Contracts.Bindings;
using AiService.Api.Contracts.Models;
using AiService.Api.Contracts.Options;
using AiService.Api.Contracts.Providers;
using AiService.Application.Repositories;
using AiService.Application.UseCases.Options;
using AiService.Application.UseCases.Providers.HealthCheck;

namespace AiService.Api.Mappers;

public static class AiResponseMapper
{
    public static AiProviderResponse ToResponse(AiProviderRecord record) =>
        new(
            record.Id,
            record.Name,
            record.ProviderType,
            record.BaseUrl,
            record.ApiKeyHint,
            record.Status,
            record.CreatedAtUtc,
            record.UpdatedAtUtc);

    public static IReadOnlyList<AiProviderResponse> ToResponse(IReadOnlyList<AiProviderRecord> records) =>
        records.Select(ToResponse).ToArray();

    public static AiProviderHealthResponse ToResponse(AiProviderHealthResult result) =>
        new(
            result.Provider.Id,
            result.Provider.Name,
            result.Provider.ProviderType,
            result.Provider.BaseUrl,
            result.Provider.Status,
            result.HealthStatus,
            result.HttpStatus,
            result.LatencyMs,
            result.Message,
            result.CheckedAtUtc);

    public static IReadOnlyList<AiProviderHealthResponse> ToResponse(IReadOnlyList<AiProviderHealthResult> results) =>
        results.Select(ToResponse).ToArray();

    public static AiModelResponse ToResponse(AiModelRecord record) =>
        new(
            record.Id,
            record.ProviderId,
            record.ProviderName,
            record.ProviderType,
            record.ModelName,
            record.DisplayName,
            record.Status,
            record.CreatedAtUtc,
            record.UpdatedAtUtc);

    public static IReadOnlyList<AiModelResponse> ToResponse(IReadOnlyList<AiModelRecord> records) =>
        records.Select(ToResponse).ToArray();

    public static AiServiceBindingResponse ToResponse(AiServiceBindingRecord record) =>
        new(
            record.Id,
            record.ServiceName,
            record.ModelId,
            record.ModelName,
            record.ModelDisplayName,
            record.ModelStatus,
            record.ProviderId,
            record.ProviderName,
            record.ProviderType,
            record.ProviderStatus,
            record.CreatedAtUtc,
            record.UpdatedAtUtc);

    public static IReadOnlyList<AiServiceBindingResponse> ToResponse(IReadOnlyList<AiServiceBindingRecord> records) =>
        records.Select(ToResponse).ToArray();

    public static AiOptionsResponse ToResponse(AiOptionsResult result) =>
        new(
            result.ProviderTypes.Select(option => new AiProviderTypeOptionResponse(option.ProviderType, option.DefaultBaseUrl)).ToArray(),
            result.BindableServices);
}
