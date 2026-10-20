namespace AiService.Application.Repositories;

/// <summary>Provider kèm API key đã mã hóa; chỉ dùng nội bộ để thử kết nối, không được map ra HTTP response.</summary>
public sealed record AiProviderCredentialRecord(AiProviderRecord Provider, string ApiKeyEncrypted);
