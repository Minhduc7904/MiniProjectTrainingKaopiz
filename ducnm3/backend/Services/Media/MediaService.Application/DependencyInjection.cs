using MediaService.Application.Actors;
using MediaService.Application.Features.Derivations;
using MediaService.Application.Features.Media.GetContent;
using MediaService.Application.Features.Media.Upload;
using MediaService.Application.Features.Usages.Create;
using MediaService.Application.Features.Usages.GetUrls;
using MediaService.Application.Abstractions.Urls;
using MediaService.Application.Urls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMediaApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var uploadOptions = configuration
            .GetSection(MediaUploadOptions.SectionName)
            .Get<MediaUploadOptions>() ?? new MediaUploadOptions();
        uploadOptions.Validate();
        var thumbnailOptions = configuration
            .GetSection(MediaThumbnailOptions.SectionName)
            .Get<MediaThumbnailOptions>() ?? new MediaThumbnailOptions();
        thumbnailOptions.Validate();

        services.AddSingleton(uploadOptions);
        services.AddSingleton(thumbnailOptions);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IActorValidator, StudentActorValidator>();
        services.AddScoped<IActorValidationService, ActorValidationService>();
        services.AddScoped<UploadMediaHandler>();
        services.AddScoped<GetMediaContentHandler>();
        services.AddScoped<CreateMediaUsageHandler>();
        services.AddScoped<GetMediaUsageUrlHandler>();
        services.AddScoped<GetMediaUsageUrlsHandler>();
        services.AddSingleton<IMediaUrlProvider, ContentEndpointMediaUrlProvider>();
        services.AddScoped<GenerateMediaThumbnailHandler>();
        services.AddScoped<GetMediaThumbnailStatusHandler>();
        services.AddScoped<RetryMediaThumbnailHandler>();

        return services;
    }
}
