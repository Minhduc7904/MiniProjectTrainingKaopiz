// File: backend/Services/Media/MediaService.Infrastructure/DependencyInjection.cs
// Mục đích: Đăng ký dependency injection cho layer hoặc service tương ứng.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Http;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Derivation;
using MediaService.Application.Services.Admins;
using MediaService.Application.Services.Storage;
using MediaService.Application.Services.Students;
using MediaService.Application.Services.Urls;
using MediaService.Infrastructure.Clients.Student;
using MediaService.Infrastructure.Clients.Admin;
using MediaService.Infrastructure.Health;
using MediaService.Infrastructure.Persistence;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Infrastructure.Persistence.Repositories;
using MediaService.Infrastructure.Persistence.Transactions;
using MediaService.Infrastructure.Services.Thumbnail;
using MediaService.Infrastructure.Services.Urls;
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
        services.AddSingleton<MinioInternalClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MinioStorageOptions>>().Value;
            return new MinioInternalClient(new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.UseSsl)
                .Build());
        });

        services.AddSingleton<MinioSigningClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MinioStorageOptions>>().Value;
            return new MinioSigningClient(new MinioClient()
                .WithEndpoint(options.PublicEndpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.PublicUseSsl)
                .Build());
        });

        services.AddSingleton<MinioStorageService>();
        services.AddSingleton<IStorage>(serviceProvider =>
            serviceProvider.GetRequiredService<MinioStorageService>());
        services.AddSingleton<IStorageHealthProbe>(serviceProvider =>
            serviceProvider.GetRequiredService<MinioStorageService>());
        services.AddSingleton<IStorageUploadPolicyProvider, MinioUploadPolicyProvider>();
        services.AddScoped<IMediaRepository, EfMediaRepository>();
        services.AddScoped<IMediaUsageRepository, EfMediaUsageRepository>();
        services.AddScoped<INotificationMediaUsageJobRepository, EfNotificationMediaUsageJobRepository>();
        services.AddScoped<IMediaUploadFinalizer, EfMediaUploadFinalizer>();
        services.AddScoped<IMediaDerivationRepository, EfMediaDerivationRepository>();
        services.AddSingleton<IMediaUrlProvider, ContentEndpointMediaUrlProvider>();
        services.AddSingleton<ITemporaryMediaFileFactory, TemporaryMediaFileFactory>();
        services.AddSingleton<IThumbnailGenerator, MediaThumbnailGenerator>();
        services.AddServiceQueryClient<IStudentLookup, StudentLookupClient>(
            configuration,
            ServiceNames.Student);
        services.AddServiceQueryClient<IAdminLookup, AdminLookupClient>(
            configuration,
            ServiceNames.Admin);
        services.AddSingleton<IDatabaseHealthProbe>(serviceProvider =>
            new MediaDatabaseHealthProbe(
                connectionString,
                serviceProvider.GetRequiredService<ILogger<MediaDatabaseHealthProbe>>()));

        return services;
    }
}
