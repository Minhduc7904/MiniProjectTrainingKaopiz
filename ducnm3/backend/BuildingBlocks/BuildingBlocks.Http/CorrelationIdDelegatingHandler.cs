using BuildingBlocks.Contracts.Api;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Http;

/// <summary>
/// HTTP handler forward correlation ID của request hiện tại sang service downstream.
/// Được <see cref="ServiceQueryClientExtensions"/> gắn tự động vào typed query client.
/// </summary>
public sealed class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    /// <summary>Thêm header khi request đang chạy trong HTTP context và caller chưa tự đặt header, sau đó chuyển request cho handler kế tiếp.</summary>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Không ghi đè header do caller chủ động truyền; nhờ đó một trace xuyên service giữ cùng định danh.
        var correlationId = httpContextAccessor.HttpContext?.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(correlationId) &&
            !request.Headers.Contains(ApiHeaderNames.CorrelationId))
        {
            request.Headers.TryAddWithoutValidation(
                ApiHeaderNames.CorrelationId,
                correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
