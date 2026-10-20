// File: backend/Services/Ai/AiService.Application/Services/ISecretProtector.cs
// Mục đích: Port mã hóa API key của AI provider để Application không phụ thuộc thuật toán cụ thể.

namespace AiService.Application.Services;

public interface ISecretProtector
{
    /// <summary>Mã hóa plain text và trả chuỗi base64 gồm nonce, tag và ciphertext.</summary>
    string Protect(string plainText);

    /// <summary>Giải mã chuỗi do <see cref="Protect"/> tạo ra; ném lỗi nếu dữ liệu bị sửa hoặc sai key.</summary>
    string Unprotect(string protectedText);

    /// <summary>Tạo hint che key, chỉ để lộ 4 ký tự cuối, dùng để admin nhận biết key đã lưu.</summary>
    string Mask(string plainText);
}
