// File: backend/Services/Ai/AiService.Application/UseCases/Bindings/Upsert/UpsertAiServiceBindingHandler.cs
// Mục đích: Gán (hoặc đổi) AI model cho một service; chỉ cho phép model và provider đang ACTIVE.

using AiService.Application.Common;
using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Domain.Constants;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Bindings.Upsert;

public sealed class UpsertAiServiceBindingHandler(
    IAiServiceBindingRepository bindingRepository,
    IAiModelRepository modelRepository,
    IAiProviderRepository providerRepository)
{
    public async Task<UpsertAiServiceBindingResult> HandleAsync(
        UpsertAiServiceBindingCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var details = new List<ApiErrorDetail>();
        var serviceName = AiServiceNames.Normalize(command.ServiceName);
        if (!AiBindableServices.IsSupported(serviceName))
        {
            details.Add(new ApiErrorDetail("serviceName", $"Service name must be one of {string.Join(", ", AiBindableServices.All)}."));
        }

        if (command.ModelId is null || command.ModelId == Guid.Empty)
        {
            details.Add(new ApiErrorDetail("modelId", "modelId must be a valid UUID."));
        }

        if (details.Count > 0 || command.ModelId is not { } modelId) throw AiErrors.ValidationFailed(details);

        var model = await modelRepository.GetAsync(modelId, cancellationToken)
            ?? throw AiErrors.ModelNotFound();
        var provider = await providerRepository.GetAsync(model.ProviderId, cancellationToken)
            ?? throw AiErrors.ProviderNotFound();
        if (model.Status != AiStatuses.Active || provider.Status != AiStatuses.Active)
        {
            throw AiErrors.ReferenceInactive();
        }

        var (binding, created) = await bindingRepository.UpsertAsync(serviceName, modelId, cancellationToken);
        return new UpsertAiServiceBindingResult(binding, created);
    }
}
