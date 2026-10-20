// File: backend/Services/Ai/AiService.Application/UseCases/Providers/Update/UpdateAiProviderHandler.cs
// Mục đích: Cập nhật AI provider; chỉ mã hóa và thay API key khi client gửi key mới.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Application.Services;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Providers.Update;

public sealed class UpdateAiProviderHandler(IAiProviderRepository repository, ISecretProtector secretProtector)
{
    public async Task<AiProviderRecord> HandleAsync(UpdateAiProviderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var details = new List<ApiErrorDetail>();
        if (command.ProviderId == Guid.Empty) details.Add(new ApiErrorDetail("providerId", "providerId must be a valid UUID."));
        var name = AiProviderValidation.NormalizeName(command.Name, details);
        var providerType = AiProviderValidation.NormalizeProviderType(command.ProviderType, details);
        var baseUrl = AiProviderValidation.ResolveBaseUrl(command.BaseUrl, providerType, details);
        var apiKey = AiProviderValidation.NormalizeApiKey(command.ApiKey, required: false, details);
        var status = AiProviderValidation.NormalizeStatus(command.Status, null, details);
        if (details.Count > 0) throw AiErrors.ValidationFailed(details);

        _ = await repository.GetAsync(command.ProviderId, cancellationToken)
            ?? throw AiErrors.ProviderNotFound();
        if (await repository.NameExistsAsync(name, command.ProviderId, cancellationToken))
        {
            throw AiErrors.ProviderNameConflict();
        }

        return await repository.UpdateAsync(
                command.ProviderId,
                name,
                providerType,
                baseUrl,
                apiKey is null ? null : secretProtector.Protect(apiKey),
                apiKey is null ? null : secretProtector.Mask(apiKey),
                status,
                cancellationToken)
            ?? throw AiErrors.ProviderNotFound();
    }
}
