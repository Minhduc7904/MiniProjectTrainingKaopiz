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
using CourseService.Application.Services.Ai;
using CourseService.Infrastructure.Clients.Ai;
using CourseService.Infrastructure.Clients.Media;
using CourseService.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        services.AddScoped<ICourseSummaryRepository>(provider => provider.GetRequiredService<EfCourseListRepository>());
        services.AddScoped<ICourseDetailsRepository, EfCourseDetailsRepository>();
        services.AddServiceQueryClient<ICourseMediaReader, CourseMediaReader>(configuration, ServiceNames.Media);
        services.AddScoped<ILessonCommandRepository, EfLessonCommandRepository>();
        services.AddScoped<ICourseCommandRepository, EfCourseCommandRepository>();
        services.AddScoped<ILearningCommandRepository, EfLearningCommandRepository>();
        services.AddScoped<IStudentLearningRepository, EfStudentLearningRepository>();
        services.AddScoped<IQuizCommandRepository, EfQuizCommandRepository>();
        services.AddScoped<IStudentQuizRepository, EfStudentQuizRepository>();
        services.AddScoped<ILessonAiRepository, EfLessonAiRepository>();
        AddLongRunningClient<IMediaContentReader, MediaContentReader>(services, configuration, ServiceNames.Media, TimeSpan.FromMinutes(2));
        AddLongRunningClient<IAiCompletionClient, AiServiceCompletionClient>(services, configuration, ServiceNames.Ai, TimeSpan.FromMinutes(5));
        services.AddHostedService<LessonAiBackgroundService>();
        services.AddSingleton<IDatabaseHealthProbe>(provider => new CourseDatabaseHealthProbe(connectionString, provider.GetRequiredService<ILogger<CourseDatabaseHealthProbe>>()));
        return services;
    }

    /// <summary>
    /// Typed client cho lời gọi chạy lâu (tải tệp lớn, LLM): timeout dài, không retry để không lặp lời gọi LLM tốn phí;
    /// việc chạy lại do worker quyết định dựa trên trạng thái trong DB.
    /// </summary>
    private static void AddLongRunningClient<TClient, TImplementation>(
        IServiceCollection services,
        IConfiguration configuration,
        string destinationService,
        TimeSpan timeout)
        where TClient : class
        where TImplementation : class, TClient
    {
        var endpoint = configuration[$"{ConfigurationSectionNames.ServiceEndpoints}:{destinationService}"];
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseAddress) || baseAddress.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException($"A valid HTTP(S) ServiceEndpoints:{destinationService} value is required.");
        }

        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdDelegatingHandler>();
        services
            .AddHttpClient<TClient, TImplementation>(client =>
            {
                client.BaseAddress = baseAddress;
                client.Timeout = timeout;
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
    }
}
