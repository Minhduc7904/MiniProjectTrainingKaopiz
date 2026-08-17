using BuildingBlocks.Contracts.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace BuildingBlocks.Http;

/// <summary>
/// Đăng ký typed HTTP client dành cho QUERY liên service: đọc base URL, forward correlation ID và áp retry/timeout tập trung.
/// Gọi từ <c>Program.cs</c>, ví dụ <c>services.AddServiceQueryClient&lt;IStudentClient, StudentClient&gt;(configuration, ServiceNames.Student)</c>.
/// </summary>
public static class ServiceQueryClientExtensions
{
    /// <summary>
    /// Đăng ký implementation của <typeparamref name="TClient"/> với DI và trả builder để caller bổ sung handler nếu thực sự cần.
    /// <paramref name="destinationService"/> xác định key <c>ServiceEndpoints:&lt;service&gt;</c>; configuration không hợp lệ sẽ fail fast khi khởi động.
    /// </summary>
    public static IHttpClientBuilder AddServiceQueryClient<TClient, TImplementation>(
        this IServiceCollection services,
        IConfiguration configuration,
        string destinationService)
        where TClient : class
        where TImplementation : class, TClient
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationService);

        // Chỉ chấp nhận absolute HTTP(S) URL để tránh typed client được cấu hình mơ hồ hoặc trỏ sang protocol ngoài phạm vi.
        var endpoint = configuration[
            $"{ConfigurationSectionNames.ServiceEndpoints}:{destinationService}"];
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseAddress) ||
            baseAddress.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException(
                $"A valid HTTP(S) ServiceEndpoints:{destinationService} value is required.");
        }

        var options = configuration
            .GetSection(ServiceQueryOptions.SectionName)
            .Get<ServiceQueryOptions>() ?? new ServiceQueryOptions();
        options.Validate();

        services.AddSingleton(options);
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdDelegatingHandler>();

        var builder = services
            .AddHttpClient<TClient, TImplementation>(client =>
            {
                client.BaseAddress = baseAddress;
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

        // Policy chỉ retry HTTP method an toàn; các lệnh thay đổi dữ liệu không bị phát lại ngoài ý muốn.
        builder.AddStandardResilienceHandler(resilience =>
        {
            resilience.TotalRequestTimeout.Timeout =
                TimeSpan.FromSeconds(options.TimeoutSeconds);
            resilience.AttemptTimeout.Timeout =
                TimeSpan.FromSeconds(options.TimeoutSeconds);
            resilience.Retry.MaxRetryAttempts = options.RetryCount;
            resilience.Retry.Delay =
                TimeSpan.FromMilliseconds(options.RetryDelayMilliseconds);
            resilience.Retry.BackoffType = DelayBackoffType.Exponential;
            resilience.Retry.UseJitter = true;
            resilience.Retry.DisableForUnsafeHttpMethods();
        });

        return builder;
    }
}
