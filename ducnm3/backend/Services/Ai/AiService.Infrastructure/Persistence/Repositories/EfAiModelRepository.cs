// File: backend/Services/Ai/AiService.Infrastructure/Persistence/Repositories/EfAiModelRepository.cs
// Mục đích: Đọc/ghi AI model qua EF Core, kèm tên và loại provider để UI hiển thị.

using AiService.Application.Common;
using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Application.UseCases.Models.GetList;
using AiService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace AiService.Infrastructure.Persistence.Repositories;

public sealed class EfAiModelRepository(AiDbContext db) : IAiModelRepository
{
    public async Task<PagedResult<AiModelRecord>> GetPagedAsync(GetAiModelsQuery query, CancellationToken cancellationToken)
    {
        var source = db.AiModels.AsNoTracking();
        if (query.ProviderId is not null) source = source.Where(model => model.ProviderId == query.ProviderId);
        if (query.Status is not null) source = source.Where(model => model.Status == query.Status);
        if (query.Search is not null)
        {
            source = source.Where(model => model.ModelName.Contains(query.Search) || model.DisplayName.Contains(query.Search));
        }

        var totalItems = await source.LongCountAsync(cancellationToken);
        var rows = await ProjectRows(source
                .OrderByDescending(model => model.CreatedAt).ThenByDescending(model => model.Id)
                .Skip(checked((query.Page - 1) * query.PageSize))
                .Take(query.PageSize))
            .ToListAsync(cancellationToken);
        return new PagedResult<AiModelRecord>(
            rows.Select(MapRow).ToArray(),
            totalItems,
            AiListPaging.TotalPages(totalItems, query.PageSize));
    }

    public async Task<AiModelRecord?> GetAsync(Guid modelId, CancellationToken cancellationToken)
    {
        var row = await ProjectRows(db.AiModels.AsNoTracking().Where(model => model.Id == modelId))
            .SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : MapRow(row);
    }

    public Task<bool> ExistsAsync(Guid providerId, string modelName, Guid? excludeModelId, CancellationToken cancellationToken) =>
        db.AiModels.AsNoTracking()
            .AnyAsync(
                model => model.ProviderId == providerId && model.ModelName == modelName && (excludeModelId == null || model.Id != excludeModelId),
                cancellationToken);

    public Task<bool> HasBindingsAsync(Guid modelId, CancellationToken cancellationToken) =>
        db.AiServiceBindings.AsNoTracking().AnyAsync(binding => binding.ModelId == modelId, cancellationToken);

    public async Task<AiModelRecord> CreateAsync(
        Guid providerId,
        string modelName,
        string displayName,
        string status,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var model = new AiModel
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            ModelName = modelName,
            DisplayName = displayName,
            Status = status,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.AiModels.Add(model);
        await SaveAsync(cancellationToken);
        return await GetAsync(model.Id, cancellationToken)
            ?? throw new InvalidOperationException("Created AI model could not be read back.");
    }

    public async Task<AiModelRecord?> UpdateAsync(
        Guid modelId,
        string modelName,
        string displayName,
        string status,
        CancellationToken cancellationToken)
    {
        var model = await db.AiModels.SingleOrDefaultAsync(item => item.Id == modelId, cancellationToken);
        if (model is null) return null;
        model.ModelName = modelName;
        model.DisplayName = displayName;
        model.Status = status;
        model.UpdatedAt = DateTime.UtcNow;
        await SaveAsync(cancellationToken);
        return await GetAsync(modelId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid modelId, CancellationToken cancellationToken)
    {
        var model = await db.AiModels.SingleOrDefaultAsync(item => item.Id == modelId, cancellationToken);
        if (model is null) return false;
        db.AiModels.Remove(model);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.IsForeignKeyViolation())
        {
            throw AiErrors.ModelInUse();
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
            throw AiErrors.ModelConflict();
        }
        catch (DbUpdateException exception) when (exception.IsForeignKeyViolation())
        {
            throw AiErrors.ProviderNotFound();
        }
    }

    private static IQueryable<AiModelRow> ProjectRows(IQueryable<AiModel> source) =>
        source.Select(model => new AiModelRow(
            model.Id,
            model.ProviderId,
            model.Provider.Name,
            model.Provider.ProviderType,
            model.ModelName,
            model.DisplayName,
            model.Status,
            model.CreatedAt,
            model.UpdatedAt));

    private static AiModelRecord MapRow(AiModelRow row) =>
        new(
            row.Id,
            row.ProviderId,
            row.ProviderName,
            row.ProviderType,
            row.ModelName,
            row.DisplayName,
            row.Status,
            DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc),
            DateTime.SpecifyKind(row.UpdatedAt, DateTimeKind.Utc));

    private sealed record AiModelRow(
        Guid Id,
        Guid ProviderId,
        string ProviderName,
        string ProviderType,
        string ModelName,
        string DisplayName,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
