using BuildingBlocks.Contracts.Health;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudentService.Application.Repositories;
using StudentService.Infrastructure.Health;
using StudentService.Infrastructure.Persistence;
using StudentService.Infrastructure.Persistence.Repositories;

namespace StudentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<StudentDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0))));
        services.AddScoped<EfStudentRepository>();
        services.AddScoped<IStudentRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<EfStudentRepository>());
        services.AddScoped<IStudentListRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<EfStudentRepository>());
        services.AddSingleton<IDatabaseHealthProbe>(serviceProvider =>
            new StudentDatabaseHealthProbe(
                connectionString,
                serviceProvider.GetRequiredService<
                    ILogger<StudentDatabaseHealthProbe>>()));

        return services;
    }
}
