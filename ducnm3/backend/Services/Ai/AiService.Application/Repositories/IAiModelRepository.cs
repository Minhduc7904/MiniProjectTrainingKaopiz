// File: backend/Services/Ai/AiService.Application/Repositories/IAiModelRepository.cs
// Mục đích: Port đọc/ghi AI model thuộc một provider.

using AiService.Application.Common;
using AiService.Application.UseCases.Models.GetList;

namespace AiService.Application.Repositories;

public interface IAiModelRepository
{
    Task<PagedResult<AiModelRecord>> GetPagedAsync(GetAiModelsQuery query, CancellationToken cancellationToken);

    Task<AiModelRecord?> GetAsync(Guid modelId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid providerId, string modelName, Guid? excludeModelId, CancellationToken cancellationToken);

    Task<bool> HasBindingsAsync(Guid modelId, CancellationToken cancellationToken);

    Task<AiModelRecord> CreateAsync(
        Guid providerId,
        string modelName,
        string displayName,
        string status,
        CancellationToken cancellationToken);

    Task<AiModelRecord?> UpdateAsync(
        Guid modelId,
        string modelName,
        string displayName,
        string status,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid modelId, CancellationToken cancellationToken);
}
