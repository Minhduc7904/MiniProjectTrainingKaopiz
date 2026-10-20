// File: backend/Services/Ai/AiService.Application/UseCases/Models/Create/CreateAiModelHandler.cs
// Mục đích: Validate và tạo AI model thuộc một provider đã tồn tại.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Domain.Constants;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Models.Create;

public sealed class CreateAiModelHandler(IAiModelRepository modelRepository, IAiProviderRepository providerRepository)
{
    public async Task<AiModelRecord> HandleAsync(CreateAiModelCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var details = new List<ApiErrorDetail>();
        if (command.ProviderId is null || command.ProviderId == Guid.Empty)
        {
            details.Add(new ApiErrorDetail("providerId", "providerId must be a valid UUID."));
        }

        var modelName = AiModelValidation.NormalizeModelName(command.ModelName, details);
        var displayName = AiModelValidation.NormalizeDisplayName(command.DisplayName, modelName, details);
        var status = AiModelValidation.NormalizeStatus(command.Status, AiStatuses.Active, details);
        if (details.Count > 0 || command.ProviderId is not { } providerId) throw AiErrors.ValidationFailed(details);

        _ = await providerRepository.GetAsync(providerId, cancellationToken)
            ?? throw AiErrors.ProviderNotFound();
        if (await modelRepository.ExistsAsync(providerId, modelName, null, cancellationToken))
        {
            throw AiErrors.ModelConflict();
        }

        return await modelRepository.CreateAsync(providerId, modelName, displayName, status, cancellationToken);
    }
}
