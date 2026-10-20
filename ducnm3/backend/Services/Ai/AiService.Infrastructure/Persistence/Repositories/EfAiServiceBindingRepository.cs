// File: backend/Services/Ai/AiService.Infrastructure/Persistence/Repositories/EfAiServiceBindingRepository.cs
// Mục đích: Đọc/ghi binding giữa service và AI model qua EF Core, kèm thông tin model và provider.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace AiService.Infrastructure.Persistence.Repositories;

public sealed class EfAiServiceBindingRepository(AiDbContext db) : IAiServiceBindingRepository
{
    public async Task<IReadOnlyList<AiServiceBindingRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await ProjectRows(db.AiServiceBindings.AsNoTracking().OrderBy(binding => binding.ServiceName))
            .ToListAsync(cancellationToken);
        return rows.Select(MapRow).ToArray();
    }

    public async Task<AiServiceBindingRecord?> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken)
    {
        var row = await ProjectRows(db.AiServiceBindings.AsNoTracking().Where(binding => binding.ServiceName == serviceName))
            .SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : MapRow(row);
    }

    public async Task<(AiServiceBindingRecord Binding, bool Created)> UpsertAsync(
        string serviceName,
        Guid modelId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var binding = await db.AiServiceBindings.SingleOrDefaultAsync(item => item.ServiceName == serviceName, cancellationToken);
        var created = binding is null;
        if (binding is null)
        {
            binding = new AiServiceBinding
            {
                Id = Guid.NewGuid(),
                ServiceName = serviceName,
                ModelId = modelId,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.AiServiceBindings.Add(binding);
        }
        else
        {
            binding.ModelId = modelId;
            binding.UpdatedAt = now;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.IsForeignKeyViolation())
        {
            throw AiErrors.ModelNotFound();
        }

        var record = await GetByServiceNameAsync(serviceName, cancellationToken)
            ?? throw new InvalidOperationException("Saved AI service binding could not be read back.");
        return (record, created);
    }

    public async Task<bool> DeleteAsync(string serviceName, CancellationToken cancellationToken)
    {
        var binding = await db.AiServiceBindings.SingleOrDefaultAsync(item => item.ServiceName == serviceName, cancellationToken);
        if (binding is null) return false;
        db.AiServiceBindings.Remove(binding);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IQueryable<AiServiceBindingRow> ProjectRows(IQueryable<AiServiceBinding> source) =>
        source.Select(binding => new AiServiceBindingRow(
            binding.Id,
            binding.ServiceName,
            binding.ModelId,
            binding.Model.ModelName,
            binding.Model.DisplayName,
            binding.Model.Status,
            binding.Model.ProviderId,
            binding.Model.Provider.Name,
            binding.Model.Provider.ProviderType,
            binding.Model.Provider.Status,
            binding.CreatedAt,
            binding.UpdatedAt));

    private static AiServiceBindingRecord MapRow(AiServiceBindingRow row) =>
        new(
            row.Id,
            row.ServiceName,
            row.ModelId,
            row.ModelName,
            row.ModelDisplayName,
            row.ModelStatus,
            row.ProviderId,
            row.ProviderName,
            row.ProviderType,
            row.ProviderStatus,
            DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc),
            DateTime.SpecifyKind(row.UpdatedAt, DateTimeKind.Utc));

    private sealed record AiServiceBindingRow(
        Guid Id,
        string ServiceName,
        Guid ModelId,
        string ModelName,
        string ModelDisplayName,
        string ModelStatus,
        Guid ProviderId,
        string ProviderName,
        string ProviderType,
        string ProviderStatus,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
