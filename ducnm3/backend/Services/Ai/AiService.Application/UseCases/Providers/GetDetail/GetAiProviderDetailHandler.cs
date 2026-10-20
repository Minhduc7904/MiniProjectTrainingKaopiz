// File: backend/Services/Ai/AiService.Application/UseCases/Providers/GetDetail/GetAiProviderDetailHandler.cs
// Mục đích: Lấy chi tiết một AI provider, không bao gồm API key.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Providers.GetDetail;

public sealed class GetAiProviderDetailHandler(IAiProviderRepository repository)
{
    public async Task<AiProviderRecord> HandleAsync(Guid providerId, CancellationToken cancellationToken)
    {
        if (providerId == Guid.Empty)
        {
            throw AiErrors.ValidationFailed([new ApiErrorDetail("providerId", "providerId must be a valid UUID.")]);
        }

        return await repository.GetAsync(providerId, cancellationToken)
            ?? throw AiErrors.ProviderNotFound();
    }
}
