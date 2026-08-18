// File: backend/Services/Notification/NotificationService.Infrastructure/DependencyInjection.cs
// Mục đích: Bind database/client options và nối các Application port với EF repository, Student client và sender adapter.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Repositories;
using NotificationService.Application.Services.Sending;
using NotificationService.Application.Services.Students;
using NotificationService.Application.UseCases.NotificationBatches;
using NotificationService.Infrastructure.Clients.Student;
using NotificationService.Infrastructure.Health;
using NotificationService.Infrastructure.Persistence.Context;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Infrastructure.Services.Sending;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration, string connectionString)
    {
        services.AddDbContextFactory<NotificationDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));
        var batchProcessingOptions = configuration
            .GetSection(NotificationBatchProcessingOptions.SectionName)
            .Get<NotificationBatchProcessingOptions>() ?? new NotificationBatchProcessingOptions();
        batchProcessingOptions.Validate();
        services.AddSingleton(batchProcessingOptions);
        services.AddServiceQueryClient<IStudentRecipientClient, StudentRecipientClient>(configuration, ServiceNames.Student);
        services.AddScoped<EfNotificationBatchRepository>();
        services.AddScoped<INotificationBatchRepository>(provider =>
            provider.GetRequiredService<EfNotificationBatchRepository>());
        services.AddScoped<INotificationBatchDispatchRepository>(provider =>
            provider.GetRequiredService<EfNotificationBatchRepository>());
        services.AddScoped<INotificationRepository, EfNotificationRepository>();
        services.AddSingleton<INotificationSender, FakeNotificationSender>();
        services.AddSingleton<IDatabaseHealthProbe>(serviceProvider => new NotificationDatabaseHealthProbe(connectionString, serviceProvider.GetRequiredService<ILogger<NotificationDatabaseHealthProbe>>()));
        return services;
    }
}
