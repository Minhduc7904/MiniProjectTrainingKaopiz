// File: backend/Services/Notification/NotificationService.Application/DependencyInjection.cs
// Mục đích: Đăng ký toàn bộ handler, extractor và options thuộc Application để API/Worker gọi đúng use case.

using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.UseCases.NotificationBatches.Create;
using NotificationService.Application.UseCases.NotificationBatches.Dispatch;
using NotificationService.Application.UseCases.NotificationBatches.GetById;
using NotificationService.Application.UseCases.NotificationBatches.GetFailedItems;
using NotificationService.Application.UseCases.NotificationBatches.Snapshot;
using NotificationService.Application.Services.Content;
using NotificationService.Application.UseCases.Notifications.Create;
using NotificationService.Application.UseCases.Notifications.GetById;

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
