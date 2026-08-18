// File: backend/Services/Notification/NotificationService.Application/DependencyInjection.cs
// Mục đích: Đăng ký dependency injection cho layer hoặc service tương ứng.

using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Features.Batches.Create;
using NotificationService.Application.Features.Batches.Dispatch;
using NotificationService.Application.Features.Batches.GetById;
using NotificationService.Application.Features.Batches.GetFailedItems;
using NotificationService.Application.Features.Batches.Snapshot;
using NotificationService.Application.Content;
using NotificationService.Application.Features.Notifications.Create;
using NotificationService.Application.Features.Notifications.GetById;

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
        services.AddSingleton<NotificationMediaReferenceExtractor>();
        services.AddScoped<CreateNotificationHandler>();
        services.AddScoped<GetNotificationByIdHandler>();
        return services;
    }
}
