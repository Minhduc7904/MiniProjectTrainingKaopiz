// File: backend/Services/Ai/AiService.Application/UseCases/Models/Delete/DeleteAiModelHandler.cs
// Mục đích: Xóa AI model khi không còn service nào đang gán.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Models.Delete;

public sealed class DeleteAiModelHandler(IAiModelRepository repository)
{
    public async Task HandleAsync(Guid modelId, CancellationToken cancellationToken)
    {
        if (modelId == Guid.Empty)
        {
            throw AiErrors.ValidationFailed([new ApiErrorDetail("modelId", "modelId must be a valid UUID.")]);
        }

        _ = await repository.GetAsync(modelId, cancellationToken)
            ?? throw AiErrors.ModelNotFound();
        if (await repository.HasBindingsAsync(modelId, cancellationToken))
        {
            throw AiErrors.ModelInUse();
        }

        if (!await repository.DeleteAsync(modelId, cancellationToken))
        {
            throw AiErrors.ModelNotFound();
        }
    }
}
