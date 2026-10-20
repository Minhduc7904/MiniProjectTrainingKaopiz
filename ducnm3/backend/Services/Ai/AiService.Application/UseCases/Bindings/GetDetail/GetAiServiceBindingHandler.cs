// File: backend/Services/Ai/AiService.Application/UseCases/Bindings/GetDetail/GetAiServiceBindingHandler.cs
// Mục đích: Lấy binding hiện tại của một service.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;

namespace AiService.Application.UseCases.Bindings.GetDetail;

public sealed class GetAiServiceBindingHandler(IAiServiceBindingRepository repository)
{
    public async Task<AiServiceBindingRecord> HandleAsync(string? serviceName, CancellationToken cancellationToken) =>
        await repository.GetByServiceNameAsync(AiServiceNames.Normalize(serviceName), cancellationToken)
            ?? throw AiErrors.BindingNotFound();
}
