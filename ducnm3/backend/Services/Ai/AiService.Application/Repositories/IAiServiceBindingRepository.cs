// File: backend/Services/Ai/AiService.Application/Repositories/IAiServiceBindingRepository.cs
// Mục đích: Port đọc/ghi binding giữa service gọi AI và AI model được chọn.

namespace AiService.Application.Repositories;

public interface IAiServiceBindingRepository
{
    Task<IReadOnlyList<AiServiceBindingRecord>> GetAllAsync(CancellationToken cancellationToken);

    Task<AiServiceBindingRecord?> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken);

    /// <summary>Tạo mới hoặc đổi model của binding theo service name; <c>Created</c> cho biết đây là binding mới.</summary>
    Task<(AiServiceBindingRecord Binding, bool Created)> UpsertAsync(
        string serviceName,
        Guid modelId,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string serviceName, CancellationToken cancellationToken);
}
