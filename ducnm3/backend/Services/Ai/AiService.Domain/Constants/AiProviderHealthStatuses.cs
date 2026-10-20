// File: backend/Services/Ai/AiService.Domain/Constants/AiProviderHealthStatuses.cs
// Mục đích: Khai báo kết quả thử kết nối tới AI provider (không lưu DB, chỉ trả về theo từng lần kiểm tra).

namespace AiService.Domain.Constants;

public static class AiProviderHealthStatuses
{
    /// <summary>Provider phản hồi 2xx với API key đang lưu.</summary>
    public const string Online = "ONLINE";

    /// <summary>Provider từ chối API key (HTTP 401/403).</summary>
    public const string AuthFailed = "AUTH_FAILED";

    /// <summary>Không kết nối được tới base URL (DNS, TLS, từ chối kết nối hoặc quá thời gian chờ).</summary>
    public const string Unreachable = "UNREACHABLE";

    /// <summary>Provider trả HTTP lỗi khác, hoặc cấu hình/API key đã lưu không dùng được.</summary>
    public const string Error = "ERROR";

    /// <summary>Provider đang tắt nên không gọi ra ngoài.</summary>
    public const string Disabled = "DISABLED";
}
