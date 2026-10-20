// File: backend/Services/Ai/AiService.Application/UseCases/Models/Update/UpdateAiModelHandler.cs
// Mục đích: Cập nhật tên, display name và trạng thái của AI model.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Models.Update;

public sealed class UpdateAiModelHandler(IAiModelRepository repository)
{
    public async Task<AiModelRecord> HandleAsync(UpdateAiModelCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var details = new List<ApiErrorDetail>();
        if (command.ModelId == Guid.Empty) details.Add(new ApiErrorDetail("modelId", "modelId must be a valid UUID."));
        var modelName = AiModelValidation.NormalizeModelName(command.ModelName, details);
        var displayName = AiModelValidation.NormalizeDisplayName(command.DisplayName, modelName, details);
        var status = AiModelValidation.NormalizeStatus(command.Status, null, details);
        if (details.Count > 0) throw AiErrors.ValidationFailed(details);

        var current = await repository.GetAsync(command.ModelId, cancellationToken)
            ?? throw AiErrors.ModelNotFound();
        if (await repository.ExistsAsync(current.ProviderId, modelName, command.ModelId, cancellationToken))
        {
            throw AiErrors.ModelConflict();
        }

        return await repository.UpdateAsync(command.ModelId, modelName, displayName, status, cancellationToken)
            ?? throw AiErrors.ModelNotFound();
    }
}
