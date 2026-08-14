using BuildingBlocks.Contracts.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CorsOptions = BuildingBlocks.Presentation.Cors.CorsOptions;

namespace BuildingBlocks.Presentation.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddLmsCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

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

    public static IApplicationBuilder UseLmsCors(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseCors(CorsPolicyNames.Frontend);
    }
}
