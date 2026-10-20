// File: backend/Services/Ai/AiService.Application/UseCases/Options/GetAiOptionsHandler.cs
// Mục đích: Trả các lựa chọn cố định (loại provider kèm base URL mặc định, service được phép gán) cho UI cấu hình.

using AiService.Application.Common;
using AiService.Domain.Constants;

namespace AiService.Application.UseCases.Options;

public sealed record AiProviderTypeOption(string ProviderType, string? DefaultBaseUrl);

public sealed record AiOptionsResult(
    IReadOnlyList<AiProviderTypeOption> ProviderTypes,
    IReadOnlyList<string> BindableServices);

public sealed class GetAiOptionsHandler
{
    private readonly AiOptionsResult _options = new(
        AiProviderTypes.All.Select(type => new AiProviderTypeOption(type, AiProviderTypes.DefaultBaseUrl(type))).ToArray(),
        AiBindableServices.All);

    public AiOptionsResult Handle() => _options;
}
