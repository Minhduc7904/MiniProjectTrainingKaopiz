// File: backend/Services/Ai/AiService.Application/Repositories/IAiProviderRepository.cs
// Mục đích: Port đọc/ghi AI provider; API key chỉ đi vào dưới dạng đã mã hóa và không bao giờ trả ra.

using AiService.Application.Common;
using AiService.Application.UseCases.Providers.GetList;

namespace AiService.Application.Repositories;

public interface IAiProviderRepository
{
    Task<PagedResult<AiProviderRecord>> GetPagedAsync(GetAiProvidersQuery query, CancellationToken cancellationToken);

    Task<AiProviderRecord?> GetAsync(Guid providerId, CancellationToken cancellationToken);

    /// <summary>Lấy provider kèm API key đã mã hóa để thử kết nối; không dùng cho response.</summary>
    Task<AiProviderCredentialRecord?> GetCredentialAsync(Guid providerId, CancellationToken cancellationToken);

    /// <summary>Lấy mọi provider kèm API key đã mã hóa, mới tạo trước, để thử kết nối hàng loạt.</summary>
    Task<IReadOnlyList<AiProviderCredentialRecord>> GetAllCredentialsAsync(CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, Guid? excludeProviderId, CancellationToken cancellationToken);

    Task<bool> HasModelsAsync(Guid providerId, CancellationToken cancellationToken);

    Task<AiProviderRecord> CreateAsync(
        string name,
        string providerType,
        string baseUrl,
        string apiKeyEncrypted,
        string apiKeyHint,
        string status,
        CancellationToken cancellationToken);

    /// <summary>Cập nhật provider; <paramref name="apiKeyEncrypted"/> null nghĩa là giữ nguyên key cũ.</summary>
    Task<AiProviderRecord?> UpdateAsync(
        Guid providerId,
        string name,
        string providerType,
        string baseUrl,
        string? apiKeyEncrypted,
        string? apiKeyHint,
        string status,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid providerId, CancellationToken cancellationToken);
}
