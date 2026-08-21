// File: backend/Services/Media/MediaService.Application/DependencyInjection.cs
// Mục đích: Đăng ký dependency injection cho layer hoặc service tương ứng.

using MediaService.Application.Services.Actors;
using MediaService.Application.UseCases.Media.DirectUpload.Complete;
using MediaService.Application.UseCases.Media.DirectUpload.CreateIntent;
using MediaService.Application.UseCases.Media.GetContent;
using MediaService.Application.UseCases.Media.Library;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Application.UseCases.MediaDerivations.GenerateThumbnail;
using MediaService.Application.UseCases.MediaDerivations.GetThumbnailStatus;
using MediaService.Application.UseCases.MediaDerivations.RetryThumbnail;
using MediaService.Application.UseCases.MediaUsageJobs.GetStatus;
using MediaService.Application.UseCases.MediaUsageJobs.Process;
using MediaService.Application.UseCases.MediaUsages.Create;
using MediaService.Application.UseCases.MediaUsages.DeleteByIds;
using MediaService.Application.UseCases.MediaUsages.GetUrl;
using MediaService.Application.UseCases.MediaUsages.GetIds;
using MediaService.Application.UseCases.MediaUsages.GetUrls;
using MediaService.Application.UseCases.MediaUsages.RegisterNotification;
using MediaService.Application.UseCases.MediaUsages.SynchronizeMarkdown;
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
        services.AddScoped<IActorValidator, AdminActorValidator>();
        services.AddScoped<IActorValidationService, ActorValidationService>();
        services.AddScoped<UploadMediaHandler>();
        services.AddScoped<CreateUploadIntentHandler>();
        services.AddScoped<CompleteDirectUploadHandler>();
        services.AddScoped<GetMediaContentHandler>();
        services.AddScoped<GetMediaLibraryHandler>();
        services.AddScoped<CreateMediaUsageHandler>();
        services.AddScoped<DeleteMediaUsagesByIdsHandler>();
        services.AddScoped<GetMediaUsageUrlHandler>();
        services.AddScoped<GetMediaUsageIdsByOwnersHandler>();
        services.AddScoped<GetMediaUsageUrlsHandler>();
        services.AddScoped<RegisterNotificationMediaUsagesHandler>();
        services.AddScoped<SynchronizeMarkdownMediaUsagesHandler>();
        services.AddScoped<GetNotificationMediaUsageJobStatusHandler>();
        services.AddScoped<NotificationMediaUsageJobLifecycleHandler>();
        services.AddScoped<GenerateMediaThumbnailHandler>();
        services.AddScoped<GetMediaThumbnailStatusHandler>();
        services.AddScoped<RetryMediaThumbnailHandler>();

        return services;
    }
}
