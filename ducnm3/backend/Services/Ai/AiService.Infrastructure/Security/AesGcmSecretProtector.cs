// File: backend/Services/Ai/AiService.Infrastructure/Security/AesGcmSecretProtector.cs
// Mục đích: Mã hóa/giải mã API key bằng AES-256-GCM; mỗi lần mã hóa dùng nonce ngẫu nhiên riêng.

using System.Security.Cryptography;
using System.Text;
using AiService.Application.Services;
using Microsoft.Extensions.Options;

namespace AiService.Infrastructure.Security;

public sealed class AesGcmSecretProtector : ISecretProtector
{
    public const int KeySizeInBytes = 32;
    private const int NonceSizeInBytes = 12;
    private const int TagSizeInBytes = 16;
    private const int VisibleHintCharacters = 4;
    private const int MinimumLengthForHint = 8;
    private const string HintPrefix = "****";

    private readonly byte[] _key;

    public AesGcmSecretProtector(IOptions<AiEncryptionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _key = Convert.FromBase64String(options.Value.Key);
        if (_key.Length != KeySizeInBytes)
        {
            throw new InvalidOperationException("Ai:Encryption:Key must be the base64 encoding of exactly 32 bytes.");
        }
    }

    public string Protect(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);
        var plain = Encoding.UTF8.GetBytes(plainText);
        // Layout: nonce | tag | ciphertext, nối liền để chỉ cần lưu một cột.
        var payload = new byte[NonceSizeInBytes + TagSizeInBytes + plain.Length];
        var nonce = payload.AsSpan(0, NonceSizeInBytes);
        var tag = payload.AsSpan(NonceSizeInBytes, TagSizeInBytes);
        var cipher = payload.AsSpan(NonceSizeInBytes + TagSizeInBytes);
        RandomNumberGenerator.Fill(nonce);
        using var aes = new AesGcm(_key, TagSizeInBytes);
        aes.Encrypt(nonce, plain, cipher, tag);
        CryptographicOperations.ZeroMemory(plain);
        return Convert.ToBase64String(payload);
    }

    public string Unprotect(string protectedText)
    {
        ArgumentException.ThrowIfNullOrEmpty(protectedText);
        var payload = Convert.FromBase64String(protectedText);
        if (payload.Length < NonceSizeInBytes + TagSizeInBytes)
        {
            throw new CryptographicException("Protected secret is malformed.");
        }

        var nonce = payload.AsSpan(0, NonceSizeInBytes);
        var tag = payload.AsSpan(NonceSizeInBytes, TagSizeInBytes);
        var cipher = payload.AsSpan(NonceSizeInBytes + TagSizeInBytes);
        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(_key, TagSizeInBytes);
        aes.Decrypt(nonce, cipher, tag, plain);
        try
        {
            return Encoding.UTF8.GetString(plain);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plain);
        }
    }

    public string Mask(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);
        // Key quá ngắn thì không để lộ ký tự nào, tránh hint tiết lộ gần hết key.
        return plainText.Length < MinimumLengthForHint
            ? HintPrefix
            : HintPrefix + plainText[^VisibleHintCharacters..];
    }
}
