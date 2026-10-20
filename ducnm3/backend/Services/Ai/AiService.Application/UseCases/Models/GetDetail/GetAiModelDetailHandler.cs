// File: backend/Services/Ai/AiService.Application/UseCases/Models/GetDetail/GetAiModelDetailHandler.cs
// Mục đích: Lấy chi tiết một AI model kèm thông tin provider.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Models.GetDetail;

public sealed class GetAiModelDetailHandler(IAiModelRepository repository)
{
    public async Task<AiModelRecord> HandleAsync(Guid modelId, CancellationToken cancellationToken)
    {
        if (modelId == Guid.Empty)
        {
            throw AiErrors.ValidationFailed([new ApiErrorDetail("modelId", "modelId must be a valid UUID.")]);
        }

        return await repository.GetAsync(modelId, cancellationToken)
            ?? throw AiErrors.ModelNotFound();
    }
}
