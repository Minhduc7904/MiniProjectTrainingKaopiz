// File: backend/Services/Ai/AiService.Api/Endpoints/RouteIdParser.cs
// Mục đích: Parse UUID từ route; giá trị sai trả Guid.Empty để handler tạo lỗi validation 400 thống nhất.

namespace AiService.Api.Endpoints;

internal static class RouteIdParser
{
    public static Guid ParseOrEmpty(string? value) =>
        Guid.TryParse(value, out var id) ? id : Guid.Empty;
}
