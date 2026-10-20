// File: backend/Services/Ai/AiService.Infrastructure/DependencyInjection.cs
// Mục đích: Đăng ký persistence, mã hóa secret và health probe của AI Service.

using AiService.Application.Repositories;
using AiService.Application.Services;
using AiService.Infrastructure.Health;
using AiService.Infrastructure.Probe;
using AiService.Infrastructure.Persistence;
using AiService.Infrastructure.Persistence.Repositories;
using AiService.Infrastructure.Security;
using BuildingBlocks.Contracts.Health;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAiInfrastructure(this IServiceCollection services, IConfiguration configuration, string connectionString)
    {
        services.AddDbContext<AiDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));
        services.AddScoped<IAiProviderRepository, EfAiProviderRepository>();
        services.AddScoped<IAiModelRepository, EfAiModelRepository>();
        services.AddScoped<IAiServiceBindingRepository, EfAiServiceBindingRepository>();

        services.AddSingleton<IValidateOptions<AiEncryptionOptions>, AiEncryptionOptionsValidator>();
        services
            .AddOptions<AiEncryptionOptions>()
            .Bind(configuration.GetSection(AiEncryptionOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<ISecretProtector, AesGcmSecretProtector>();

        // HttpClient dùng chung; không theo redirect để Authorization không bị gửi sang host khác.
        services.AddSingleton<IAiProviderProbe>(_ => new HttpAiProviderProbe(new HttpClient(new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            ConnectTimeout = HttpAiProviderProbe.Timeout,
        })
        {
            Timeout = Timeout.InfiniteTimeSpan,
        }));

        services.AddSingleton<IDatabaseHealthProbe>(provider => new AiDatabaseHealthProbe(connectionString, provider.GetRequiredService<ILogger<AiDatabaseHealthProbe>>()));
        return services;
    }
}
