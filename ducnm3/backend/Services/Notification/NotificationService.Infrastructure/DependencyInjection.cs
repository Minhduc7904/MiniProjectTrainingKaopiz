using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Infrastructure.Clients.Student;
using NotificationService.Infrastructure.Health;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Sending;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration, string connectionString)
    {
        services.AddDbContext<NotificationDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));
        services.AddServiceQueryClient<IStudentRecipientClient, StudentRecipientClient>(configuration, ServiceNames.Student);
        services.AddScoped<INotificationBatchRepository, EfNotificationBatchRepository>();
        services.AddSingleton<INotificationSender, FakeNotificationSender>();
        services.AddSingleton<IDatabaseHealthProbe>(serviceProvider => new NotificationDatabaseHealthProbe(connectionString, serviceProvider.GetRequiredService<ILogger<NotificationDatabaseHealthProbe>>()));
        return services;
    }
}
