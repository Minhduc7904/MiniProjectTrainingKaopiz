// File: backend/Services/Ai/AiService.Application/UseCases/Models/GetList/GetAiModelsHandler.cs
// Mục đích: Điều phối use case lấy AI model theo phân trang.

using AiService.Application.Common;
using AiService.Application.Repositories;

namespace AiService.Application.UseCases.Models.GetList;

public sealed class GetAiModelsHandler(IAiModelRepository repository)
{
    public Task<PagedResult<AiModelRecord>> HandleAsync(GetAiModelsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return repository.GetPagedAsync(query, cancellationToken);
    }
}
