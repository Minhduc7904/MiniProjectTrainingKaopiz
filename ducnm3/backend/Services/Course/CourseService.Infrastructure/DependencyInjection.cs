// File: backend/Services/Course/CourseService.Infrastructure/DependencyInjection.cs
// Mục đích: Đăng ký persistence và infrastructure adapter của Course Service.

using BuildingBlocks.Contracts.Health;
using CourseService.Application.Repositories;
using CourseService.Infrastructure.Health;
using CourseService.Infrastructure.Persistence;
using CourseService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CourseService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCourseInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CourseDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));
        services.AddScoped<EfCourseListRepository>();
        services.AddScoped<ICourseListRepository>(provider => provider.GetRequiredService<EfCourseListRepository>());
        services.AddScoped<ICourseDetailsRepository, EfCourseDetailsRepository>();
        services.AddSingleton<IDatabaseHealthProbe>(provider => new CourseDatabaseHealthProbe(connectionString, provider.GetRequiredService<ILogger<CourseDatabaseHealthProbe>>()));
        return services;
    }
}
