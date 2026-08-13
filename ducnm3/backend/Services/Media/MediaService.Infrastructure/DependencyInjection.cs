using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Http;
using MediaService.Application.Abstractions.Clients;
using MediaService.Application.Abstractions.Persistence;
using MediaService.Application.Abstractions.Storage;
using MediaService.Infrastructure.Clients.Student;
using MediaService.Infrastructure.Health;
using MediaService.Infrastructure.Persistence;
using MediaService.Infrastructure.Storage.Minio;
using Microsoft.EntityFrameworkCore;
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
        services.AddDbContext<MediaDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0))));

        services.AddSingleton<IValidateOptions<MinioStorageOptions>, MinioStorageOptionsValidator>();
        services
            .AddOptions<MinioStorageOptions>()
            .Bind(configuration.GetSection(MinioStorageOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<MinioObjectKeyGenerator>();
        services.AddSingleton<IStorageLocationAllocator, MinioStorageLocationAllocator>();
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
        services.AddScoped<IMediaRepository, EfMediaRepository>();
        services.AddServiceQueryClient<IStudentLookup, StudentLookupClient>(
            configuration,
            ServiceNames.Student);
        services.AddSingleton<IDatabaseHealthProbe>(serviceProvider =>
            new MediaDatabaseHealthProbe(
                connectionString,
                serviceProvider.GetRequiredService<ILogger<MediaDatabaseHealthProbe>>()));

        return services;
    }
}
