// File: backend/Services/Ai/AiService.Application/UseCases/Bindings/Delete/DeleteAiServiceBindingHandler.cs
// Mục đích: Gỡ AI model đã gán khỏi một service.

using AiService.Application.Common.Errors;
using AiService.Application.Repositories;

namespace AiService.Application.UseCases.Bindings.Delete;

public sealed class DeleteAiServiceBindingHandler(IAiServiceBindingRepository repository)
{
    public async Task HandleAsync(string? serviceName, CancellationToken cancellationToken)
    {
        if (!await repository.DeleteAsync(AiServiceNames.Normalize(serviceName), cancellationToken))
        {
            throw AiErrors.BindingNotFound();
        }
    }
}
