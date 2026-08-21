// File: backend/Services/Course/CourseService.Infrastructure/DependencyInjection.cs
// Mục đích: Đăng ký persistence và infrastructure adapter của Course Service.

using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Http;
using CourseService.Application.Repositories;
using CourseService.Infrastructure.Health;
using CourseService.Infrastructure.Persistence;
using CourseService.Infrastructure.Persistence.Repositories;
using CourseService.Application.Services.Media;
using CourseService.Infrastructure.Clients.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CourseService.Application.UseCases.Learning;

namespace CourseService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCourseInfrastructure(this IServiceCollection services, IConfiguration configuration, string connectionString)
    {
        services.AddDbContext<CourseDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));
        services.AddScoped<EfCourseListRepository>();
        services.AddScoped<ICourseListRepository>(provider => provider.GetRequiredService<EfCourseListRepository>());
        services.AddScoped<ICourseDetailsRepository, EfCourseDetailsRepository>();
        services.AddServiceQueryClient<ICourseMediaReader, CourseMediaReader>(configuration, ServiceNames.Media);
        services.AddScoped<ILessonCommandRepository, EfLessonCommandRepository>();
        services.AddScoped<ICourseCommandRepository, EfCourseCommandRepository>();
        services.AddScoped<ILearningCommandRepository, EfLearningCommandRepository>();
        services.AddScoped<IStudentLearningRepository, EfStudentLearningRepository>();
        services.AddSingleton<IDatabaseHealthProbe>(provider => new CourseDatabaseHealthProbe(connectionString, provider.GetRequiredService<ILogger<CourseDatabaseHealthProbe>>()));
        return services;
    }
}
