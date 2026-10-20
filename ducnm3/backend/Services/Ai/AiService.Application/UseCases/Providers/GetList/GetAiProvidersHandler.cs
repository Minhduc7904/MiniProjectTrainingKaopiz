// File: backend/Services/Ai/AiService.Application/UseCases/Providers/GetList/GetAiProvidersHandler.cs
// Mục đích: Điều phối use case lấy AI provider theo phân trang.

using AiService.Application.Common;
using AiService.Application.Repositories;

namespace AiService.Application.UseCases.Providers.GetList;

public sealed class GetAiProvidersHandler(IAiProviderRepository repository)
{
    public Task<PagedResult<AiProviderRecord>> HandleAsync(GetAiProvidersQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return repository.GetPagedAsync(query, cancellationToken);
    }
}
