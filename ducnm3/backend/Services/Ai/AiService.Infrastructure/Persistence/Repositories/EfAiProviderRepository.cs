// File: backend/Services/Ai/AiService.Infrastructure/Persistence/Repositories/EfAiProviderRepository.cs
// Mục đích: Đọc/ghi AI provider qua EF Core; chỉ trả hint của API key, không bao giờ đọc ciphertext ra ngoài.

using AiService.Application.Common;
using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Application.UseCases.Providers.GetList;
using AiService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace AiService.Infrastructure.Persistence.Repositories;

public sealed class EfAiProviderRepository(AiDbContext db) : IAiProviderRepository
{
    public async Task<PagedResult<AiProviderRecord>> GetPagedAsync(GetAiProvidersQuery query, CancellationToken cancellationToken)
    {
        var source = db.AiProviders.AsNoTracking();
        if (query.Status is not null) source = source.Where(provider => provider.Status == query.Status);
        if (query.ProviderType is not null) source = source.Where(provider => provider.ProviderType == query.ProviderType);
        if (query.Search is not null) source = source.Where(provider => provider.Name.Contains(query.Search));
        var totalItems = await source.LongCountAsync(cancellationToken);
        var rows = await source
            .OrderByDescending(provider => provider.CreatedAt).ThenByDescending(provider => provider.Id)
            .Skip(checked((query.Page - 1) * query.PageSize))
            .Take(query.PageSize)
            .Select(provider => new AiProviderRow(provider.Id, provider.Name, provider.ProviderType, provider.BaseUrl, provider.ApiKeyHint, provider.Status, provider.CreatedAt, provider.UpdatedAt))
            .ToListAsync(cancellationToken);
        return new PagedResult<AiProviderRecord>(
            rows.Select(MapRow).ToArray(),
            totalItems,
            AiListPaging.TotalPages(totalItems, query.PageSize));
    }

    public async Task<AiProviderRecord?> GetAsync(Guid providerId, CancellationToken cancellationToken)
    {
        var row = await db.AiProviders.AsNoTracking()
            .Where(provider => provider.Id == providerId)
            .Select(provider => new AiProviderRow(provider.Id, provider.Name, provider.ProviderType, provider.BaseUrl, provider.ApiKeyHint, provider.Status, provider.CreatedAt, provider.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : MapRow(row);
    }

    public Task<bool> NameExistsAsync(string name, Guid? excludeProviderId, CancellationToken cancellationToken) =>
        db.AiProviders.AsNoTracking()
            .AnyAsync(provider => provider.Name == name && (excludeProviderId == null || provider.Id != excludeProviderId), cancellationToken);

    public Task<bool> HasModelsAsync(Guid providerId, CancellationToken cancellationToken) =>
        db.AiModels.AsNoTracking().AnyAsync(model => model.ProviderId == providerId, cancellationToken);

    public async Task<AiProviderRecord> CreateAsync(
        string name,
        string providerType,
        string baseUrl,
        string apiKeyEncrypted,
        string apiKeyHint,
        string status,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var provider = new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProviderType = providerType,
            BaseUrl = baseUrl,
            ApiKeyEncrypted = apiKeyEncrypted,
            ApiKeyHint = apiKeyHint,
            Status = status,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.AiProviders.Add(provider);
        await SaveAsync(cancellationToken);
        return MapEntity(provider);
    }

    public async Task<AiProviderRecord?> UpdateAsync(
        Guid providerId,
        string name,
        string providerType,
        string baseUrl,
        string? apiKeyEncrypted,
        string? apiKeyHint,
        string status,
        CancellationToken cancellationToken)
    {
        var provider = await db.AiProviders.SingleOrDefaultAsync(item => item.Id == providerId, cancellationToken);
        if (provider is null) return null;
        provider.Name = name;
        provider.ProviderType = providerType;
        provider.BaseUrl = baseUrl;
        provider.Status = status;
        if (apiKeyEncrypted is not null && apiKeyHint is not null)
        {
            provider.ApiKeyEncrypted = apiKeyEncrypted;
            provider.ApiKeyHint = apiKeyHint;
        }

        provider.UpdatedAt = DateTime.UtcNow;
        await SaveAsync(cancellationToken);
        return MapEntity(provider);
    }

    public async Task<bool> DeleteAsync(Guid providerId, CancellationToken cancellationToken)
    {
        var provider = await db.AiProviders.SingleOrDefaultAsync(item => item.Id == providerId, cancellationToken);
        if (provider is null) return false;
        db.AiProviders.Remove(provider);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.IsForeignKeyViolation())
        {
            throw AiErrors.ProviderInUse();
        }

        return true;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.IsDuplicateKey())
        {
            throw AiErrors.ProviderNameConflict();
        }
    }

    private static AiProviderRecord MapEntity(AiProvider provider) =>
        MapRow(new AiProviderRow(provider.Id, provider.Name, provider.ProviderType, provider.BaseUrl, provider.ApiKeyHint, provider.Status, provider.CreatedAt, provider.UpdatedAt));

    private static AiProviderRecord MapRow(AiProviderRow row) =>
        new(
            row.Id,
            row.Name,
            row.ProviderType,
            row.BaseUrl,
            row.ApiKeyHint,
            row.Status,
            DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc),
            DateTime.SpecifyKind(row.UpdatedAt, DateTimeKind.Utc));

    private sealed record AiProviderRow(
        Guid Id,
        string Name,
        string ProviderType,
        string BaseUrl,
        string ApiKeyHint,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
