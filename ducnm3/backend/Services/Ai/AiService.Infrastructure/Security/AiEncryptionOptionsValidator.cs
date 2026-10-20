// File: backend/Services/Ai/AiService.Infrastructure/Security/AiEncryptionOptionsValidator.cs
// Mục đích: Kiểm tra master key mã hóa lúc khởi động để service fail-fast thay vì lỗi khi lưu key đầu tiên.

using Microsoft.Extensions.Options;

namespace AiService.Infrastructure.Security;

public sealed class AiEncryptionOptionsValidator : IValidateOptions<AiEncryptionOptions>
{
    public ValidateOptionsResult Validate(string? name, AiEncryptionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Key))
        {
            return ValidateOptionsResult.Fail("Ai:Encryption:Key is required.");
        }

        var buffer = new byte[AesGcmSecretProtector.KeySizeInBytes];
        if (!Convert.TryFromBase64String(options.Key, buffer, out var written) || written != AesGcmSecretProtector.KeySizeInBytes)
        {
            return ValidateOptionsResult.Fail("Ai:Encryption:Key must be the base64 encoding of exactly 32 bytes.");
        }

        return ValidateOptionsResult.Success;
    }
}
