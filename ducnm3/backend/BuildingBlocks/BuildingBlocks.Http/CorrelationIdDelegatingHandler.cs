using BuildingBlocks.Contracts.Api;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Http;

public sealed class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
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
