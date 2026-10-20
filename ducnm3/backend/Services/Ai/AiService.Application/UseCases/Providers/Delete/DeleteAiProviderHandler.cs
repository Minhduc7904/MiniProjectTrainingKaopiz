// File: backend/Services/Ai/AiService.Application/UseCases/Providers/Delete/DeleteAiProviderHandler.cs
// Mục đích: Xóa AI provider khi không còn model nào tham chiếu.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Providers.Delete;

public sealed class DeleteAiProviderHandler(IAiProviderRepository repository)
{
    public async Task HandleAsync(Guid providerId, CancellationToken cancellationToken)
    {
        if (providerId == Guid.Empty)
        {
            throw AiErrors.ValidationFailed([new ApiErrorDetail("providerId", "providerId must be a valid UUID.")]);
        }

        _ = await repository.GetAsync(providerId, cancellationToken)
            ?? throw AiErrors.ProviderNotFound();
        if (await repository.HasModelsAsync(providerId, cancellationToken))
        {
            throw AiErrors.ProviderInUse();
        }

        if (!await repository.DeleteAsync(providerId, cancellationToken))
        {
            throw AiErrors.ProviderNotFound();
        }
    }
}
