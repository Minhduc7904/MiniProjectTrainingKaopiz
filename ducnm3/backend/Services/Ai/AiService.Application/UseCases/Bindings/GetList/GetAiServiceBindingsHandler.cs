// File: backend/Services/Ai/AiService.Application/UseCases/Bindings/GetList/GetAiServiceBindingsHandler.cs
// Mục đích: Lấy toàn bộ binding; số lượng bị chặn bởi danh sách service nên không cần phân trang.

using AiService.Application.Repositories;

namespace AiService.Application.UseCases.Bindings.GetList;

public sealed class GetAiServiceBindingsHandler(IAiServiceBindingRepository repository)
{
    public Task<IReadOnlyList<AiServiceBindingRecord>> HandleAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);
}
