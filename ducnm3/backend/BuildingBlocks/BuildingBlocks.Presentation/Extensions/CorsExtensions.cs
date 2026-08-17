using BuildingBlocks.Contracts.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CorsOptions = BuildingBlocks.Presentation.Cors.CorsOptions;

namespace BuildingBlocks.Presentation.Extensions;

/// <summary>Đăng ký và áp dụng CORS policy chung cho frontend LMS.</summary>
public static class CorsExtensions
{
    /// <summary>
    /// Đọc <c>Cors:AllowedOrigins</c>, chuẩn hóa URL và đăng ký policy <c>frontend</c>.
    /// Trả về DI collection; ném lỗi khi không còn origin hợp lệ để tránh API chạy với policy sai.
    /// </summary>
    public static IServiceCollection AddLmsCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bỏ origin rỗng, slash cuối và bản sao không phân biệt hoa thường trước khi cấp quyền CORS.
        var origins = (configuration
                .GetSection(ConfigurationSectionNames.Cors)
                .Get<CorsOptions>()
                ?.AllowedOrigins ?? [])
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (origins.Length == 0)
        {
            throw new InvalidOperationException(
                "Cors:AllowedOrigins must contain at least one frontend origin.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(
                CorsPolicyNames.Frontend,
                policy =>
                {
                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders(ApiHeaderNames.CorrelationId);
                });
        });

        return services;
    }

    /// <summary>Áp dụng policy frontend đã đăng ký vào request pipeline và trả builder cho bước kế tiếp.</summary>
    public static IApplicationBuilder UseLmsCors(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseCors(CorsPolicyNames.Frontend);
    }
}
