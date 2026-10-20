// File: backend/Services/Ai/AiService.Application/UseCases/Providers/Create/CreateAiProviderHandler.cs
// Mục đích: Validate, mã hóa API key và tạo AI provider; key không bao giờ được lưu hoặc trả về dạng plain text.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Application.Services;
using AiService.Domain.Constants;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Providers.Create;

public sealed class CreateAiProviderHandler(IAiProviderRepository repository, ISecretProtector secretProtector)
{
    public async Task<AiProviderRecord> HandleAsync(CreateAiProviderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var details = new List<ApiErrorDetail>();
        var name = AiProviderValidation.NormalizeName(command.Name, details);
        var providerType = AiProviderValidation.NormalizeProviderType(command.ProviderType, details);
        var baseUrl = AiProviderValidation.ResolveBaseUrl(command.BaseUrl, providerType, details);
        var apiKey = AiProviderValidation.NormalizeApiKey(command.ApiKey, required: true, details);
        var status = AiProviderValidation.NormalizeStatus(command.Status, AiStatuses.Active, details);
        if (details.Count > 0 || apiKey is null) throw AiErrors.ValidationFailed(details);

        if (await repository.NameExistsAsync(name, null, cancellationToken))
        {
            throw AiErrors.ProviderNameConflict();
        }

        return await repository.CreateAsync(
            name,
            providerType,
            baseUrl,
            secretProtector.Protect(apiKey),
            secretProtector.Mask(apiKey),
            status,
            cancellationToken);
    }
}
