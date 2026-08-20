// File: backend/Services/Course/CourseService.Application/DependencyInjection.cs
// Mục đích: Đăng ký dependency injection cho use case thuộc Application layer.

using CourseService.Application.UseCases.Courses.Export;
using CourseService.Application.UseCases.Courses.GetDetails;
using CourseService.Application.UseCases.Courses.GetList;
using CourseService.Application.UseCases.Lessons.Create;
using Microsoft.Extensions.DependencyInjection;

namespace CourseService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCourseApplication(this IServiceCollection services)
    {
        services.AddScoped<GetCoursesHandler>();
        services.AddScoped<ExportCoursesHandler>();
        services.AddScoped<BufferedCourseExportHandler>();
        services.AddScoped<GetCourseDetailsHandler>();
        services.AddScoped<CreateLessonHandler>();
        return services;
    }
}
