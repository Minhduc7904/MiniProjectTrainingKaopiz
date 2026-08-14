using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Features.Batches.Create;
using NotificationService.Application.Features.Batches.Dispatch;
using NotificationService.Application.Features.Batches.GetById;
using NotificationService.Application.Features.Batches.GetFailedItems;
using NotificationService.Application.Features.Batches.Snapshot;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<CreateNotificationBatchHandler>();
        services.AddScoped<GetNotificationBatchByIdHandler>();
        services.AddScoped<GetNotificationBatchFailedItemsHandler>();
        services.AddScoped<DispatchNotificationBatchHandler>();
        services.AddScoped<SnapshotNotificationBatchHandler>();
        return services;
    }
}
