using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Features.Batches.Create;
using NotificationService.Application.Features.Batches.Dispatch;
using NotificationService.Application.Features.Batches.GetById;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<CreateNotificationBatchHandler>();
        services.AddScoped<GetNotificationBatchByIdHandler>();
        services.AddScoped<DispatchNotificationBatchHandler>();
        return services;
    }
}
