using AiService.Application.Common;
using AiService.Application.Repositories;
using AiService.Application.UseCases.Models.GetList;
using AiService.Application.UseCases.Providers.GetList;

namespace AiService.UnitTests.Support;

/// <summary>Repository in-memory dùng chung cho unit test handler; giữ state để test kiểm tra thứ đã được lưu.</summary>
internal sealed class InMemoryAiStore
{
    public Dictionary<Guid, StoredProvider> Providers { get; } = [];
    public Dictionary<Guid, AiModelRecord> Models { get; } = [];
    public Dictionary<string, AiServiceBindingRecord> Bindings { get; } = [];

    public InMemoryAiProviderRepository ProviderRepository => new(this);
    public InMemoryAiModelRepository ModelRepository => new(this);
    public InMemoryAiServiceBindingRepository BindingRepository => new(this);

    public AiProviderRecord AddProvider(string name = "OpenAI main", string status = "ACTIVE", string providerType = "OPENAI")
    {
        var now = DateTime.UtcNow;
        var record = new AiProviderRecord(Guid.NewGuid(), name, providerType, "https://api.openai.com/v1", "****abcd", status, now, now);
        Providers[record.Id] = new StoredProvider(record, "enc:sk-original-abcd");
        return record;
    }

    public AiModelRecord AddModel(AiProviderRecord provider, string modelName = "gpt-4o", string status = "ACTIVE")
    {
        var now = DateTime.UtcNow;
        var record = new AiModelRecord(Guid.NewGuid(), provider.Id, provider.Name, provider.ProviderType, modelName, modelName, status, now, now);
        Models[record.Id] = record;
        return record;
    }
}

internal sealed record StoredProvider(AiProviderRecord Record, string ApiKeyEncrypted);

internal sealed class InMemoryAiProviderRepository(InMemoryAiStore store) : IAiProviderRepository
{
    public Task<PagedResult<AiProviderRecord>> GetPagedAsync(GetAiProvidersQuery query, CancellationToken cancellationToken)
    {
        var items = store.Providers.Values.Select(item => item.Record).ToArray();
        return Task.FromResult(new PagedResult<AiProviderRecord>(items, items.Length, items.Length == 0 ? 0 : 1));
    }

    public Task<AiProviderRecord?> GetAsync(Guid providerId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Providers.TryGetValue(providerId, out var stored) ? stored.Record : null);

    public Task<bool> NameExistsAsync(string name, Guid? excludeProviderId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Providers.Values.Any(item =>
            string.Equals(item.Record.Name, name, StringComparison.OrdinalIgnoreCase) && item.Record.Id != excludeProviderId));

    public Task<bool> HasModelsAsync(Guid providerId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Models.Values.Any(model => model.ProviderId == providerId));

    public Task<AiProviderRecord> CreateAsync(string name, string providerType, string baseUrl, string apiKeyEncrypted, string apiKeyHint, string status, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var record = new AiProviderRecord(Guid.NewGuid(), name, providerType, baseUrl, apiKeyHint, status, now, now);
        store.Providers[record.Id] = new StoredProvider(record, apiKeyEncrypted);
        return Task.FromResult(record);
    }

    public Task<AiProviderRecord?> UpdateAsync(Guid providerId, string name, string providerType, string baseUrl, string? apiKeyEncrypted, string? apiKeyHint, string status, CancellationToken cancellationToken)
    {
        if (!store.Providers.TryGetValue(providerId, out var stored)) return Task.FromResult<AiProviderRecord?>(null);
        var record = stored.Record with
        {
            Name = name,
            ProviderType = providerType,
            BaseUrl = baseUrl,
            ApiKeyHint = apiKeyHint ?? stored.Record.ApiKeyHint,
            Status = status,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        store.Providers[providerId] = new StoredProvider(record, apiKeyEncrypted ?? stored.ApiKeyEncrypted);
        return Task.FromResult<AiProviderRecord?>(record);
    }

    public Task<bool> DeleteAsync(Guid providerId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Providers.Remove(providerId));
}

internal sealed class InMemoryAiModelRepository(InMemoryAiStore store) : IAiModelRepository
{
    public Task<PagedResult<AiModelRecord>> GetPagedAsync(GetAiModelsQuery query, CancellationToken cancellationToken)
    {
        var items = store.Models.Values.ToArray();
        return Task.FromResult(new PagedResult<AiModelRecord>(items, items.Length, items.Length == 0 ? 0 : 1));
    }

    public Task<AiModelRecord?> GetAsync(Guid modelId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Models.GetValueOrDefault(modelId));

    public Task<bool> ExistsAsync(Guid providerId, string modelName, Guid? excludeModelId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Models.Values.Any(model =>
            model.ProviderId == providerId
            && string.Equals(model.ModelName, modelName, StringComparison.OrdinalIgnoreCase)
            && model.Id != excludeModelId));

    public Task<bool> HasBindingsAsync(Guid modelId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Bindings.Values.Any(binding => binding.ModelId == modelId));

    public Task<AiModelRecord> CreateAsync(Guid providerId, string modelName, string displayName, string status, CancellationToken cancellationToken)
    {
        var provider = store.Providers[providerId].Record;
        var now = DateTime.UtcNow;
        var record = new AiModelRecord(Guid.NewGuid(), providerId, provider.Name, provider.ProviderType, modelName, displayName, status, now, now);
        store.Models[record.Id] = record;
        return Task.FromResult(record);
    }

    public Task<AiModelRecord?> UpdateAsync(Guid modelId, string modelName, string displayName, string status, CancellationToken cancellationToken)
    {
        if (!store.Models.TryGetValue(modelId, out var current)) return Task.FromResult<AiModelRecord?>(null);
        var record = current with { ModelName = modelName, DisplayName = displayName, Status = status, UpdatedAtUtc = DateTime.UtcNow };
        store.Models[modelId] = record;
        return Task.FromResult<AiModelRecord?>(record);
    }

    public Task<bool> DeleteAsync(Guid modelId, CancellationToken cancellationToken) =>
        Task.FromResult(store.Models.Remove(modelId));
}

internal sealed class InMemoryAiServiceBindingRepository(InMemoryAiStore store) : IAiServiceBindingRepository
{
    public Task<IReadOnlyList<AiServiceBindingRecord>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AiServiceBindingRecord>>(store.Bindings.Values.OrderBy(binding => binding.ServiceName).ToArray());

    public Task<AiServiceBindingRecord?> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken) =>
        Task.FromResult(store.Bindings.GetValueOrDefault(serviceName));

    public Task<(AiServiceBindingRecord Binding, bool Created)> UpsertAsync(string serviceName, Guid modelId, CancellationToken cancellationToken)
    {
        var model = store.Models[modelId];
        var provider = store.Providers[model.ProviderId].Record;
        var now = DateTime.UtcNow;
        var created = !store.Bindings.TryGetValue(serviceName, out var existing);
        var record = new AiServiceBindingRecord(
            existing?.Id ?? Guid.NewGuid(),
            serviceName,
            modelId,
            model.ModelName,
            model.DisplayName,
            model.Status,
            provider.Id,
            provider.Name,
            provider.ProviderType,
            provider.Status,
            existing?.CreatedAtUtc ?? now,
            now);
        store.Bindings[serviceName] = record;
        return Task.FromResult((record, created));
    }

    public Task<bool> DeleteAsync(string serviceName, CancellationToken cancellationToken) =>
        Task.FromResult(store.Bindings.Remove(serviceName));
}
