// File: backend/Services/Ai/AiService.Infrastructure/Security/AiEncryptionOptions.cs
// Mục đích: Cấu hình master key mã hóa API key của AI provider, đọc từ section Ai:Encryption.

namespace AiService.Infrastructure.Security;

public sealed class AiEncryptionOptions
{
    public const string SectionName = "Ai:Encryption";

    /// <summary>Base64 của đúng 32 byte; lấy từ biến môi trường Ai__Encryption__Key, không được đặt trong appsettings.</summary>
    public string Key { get; set; } = string.Empty;
}
