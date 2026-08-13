using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Http;
using MediaService.Application.Actors;
using MediaService.Application.Persistence;
using MediaService.Application.Storage;
using MediaService.Application.Upload;
using MediaService.Application.Usages;
using MediaService.Infrastructure.Health;
using MediaService.Infrastructure.Http;
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
        var uploadOptions = configuration
            .GetSection(MediaUploadOptions.SectionName)
            .Get<MediaUploadOptions>() ?? new MediaUploadOptions();
        uploadOptions.Validate();
        services.AddSingleton(uploadOptions);

        services.AddDbContext<MediaDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0))));

        services.AddSingleton<IValidateOptions<MinioStorageOptions>, MinioStorageOptionsValidator>();
        services
            .AddOptions<MinioStorageOptions>()
            .Bind(configuration.GetSection(MinioStorageOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
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
        services.AddScoped<IActorValidator, StudentActorValidator>();
        services.AddScoped<IActorValidationService, ActorValidationService>();
        services.AddScoped<UploadMediaHandler>();
        services.AddScoped<CreateMediaUsageHandler>();
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
