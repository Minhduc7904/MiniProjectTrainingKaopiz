using BuildingBlocks.Contracts.Health;
using MediaService.Application.Storage;
using MediaService.Infrastructure.Health;
using MediaService.Infrastructure.Storage.Minio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;

namespace MediaService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMediaInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        services.AddSingleton<IValidateOptions<MinioStorageOptions>, MinioStorageOptionsValidator>();
        services
            .AddOptions<MinioStorageOptions>()
            .Bind(configuration.GetSection(MinioStorageOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<MinioObjectKeyGenerator>();
        services.AddSingleton<IMinioClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MinioStorageOptions>>().Value;
            return new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.UseSsl)
                .Build();
        });

        services.AddSingleton<MinioStorageService>();
        services.AddSingleton<IStorage>(serviceProvider =>
            serviceProvider.GetRequiredService<MinioStorageService>());
        services.AddSingleton<IStorageHealthProbe>(serviceProvider =>
            serviceProvider.GetRequiredService<MinioStorageService>());
        services.AddSingleton<IDatabaseHealthProbe>(serviceProvider =>
            new MediaDatabaseHealthProbe(
                connectionString,
                serviceProvider.GetRequiredService<ILogger<MediaDatabaseHealthProbe>>()));

        return services;
    }
}
