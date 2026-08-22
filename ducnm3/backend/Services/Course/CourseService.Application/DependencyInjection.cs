// File: backend/Services/Course/CourseService.Application/DependencyInjection.cs
// Mục đích: Đăng ký dependency injection cho use case thuộc Application layer.

using CourseService.Application.UseCases.Courses.Export;
using CourseService.Application.UseCases.Courses.GetDetails;
using CourseService.Application.UseCases.Courses.GetList;
using CourseService.Application.UseCases.Courses.GetSummary;
using CourseService.Application.UseCases.Lessons.Create;
using CourseService.Application.UseCases.Lessons.Update;
using CourseService.Application.UseCases.Lessons.GetDetail;
using CourseService.Application.UseCases.Lessons.Reorder;
using CourseService.Application.UseCases.Courses.Create;
using CourseService.Application.UseCases.Courses.Update;
using CourseService.Application.Services.Content;
using CourseService.Application.UseCases.Courses.Delete;
using CourseService.Application.UseCases.Lessons.Delete;
using CourseService.Application.UseCases.Learning;
using Microsoft.Extensions.DependencyInjection;

namespace CourseService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCourseApplication(this IServiceCollection services)
    {
        services.AddScoped<GetCoursesHandler>();
        services.AddScoped<GetCoursesSummaryHandler>();
        services.AddSingleton<IMarkdownHtmlRenderer, SanitizedMarkdownHtmlRenderer>();
        services.AddScoped<ExportCoursesHandler>();
        services.AddScoped<BufferedCourseExportHandler>();
        services.AddScoped<GetCourseDetailsHandler>();
        services.AddScoped<CreateLessonHandler>();
        services.AddScoped<CreateCourseHandler>();
        services.AddScoped<UpdateCourseHandler>();
        services.AddScoped<DeleteCourseHandler>();
        services.AddScoped<DeleteLessonHandler>();
        services.AddScoped<UpdateLessonHandler>();
        services.AddScoped<GetLessonDetailHandler>();
        services.AddScoped<ReorderLessonsHandler>();
        services.AddScoped<EnrollCourseHandler>();
        services.AddScoped<CompleteLessonProgressHandler>();
        services.AddScoped<GetStudentEnrollmentsHandler>();
        services.AddScoped<GetStudentCourseCatalogHandler>();
        services.AddScoped<GetStudentEnrollmentDetailHandler>();
        services.AddScoped<GetStudentLessonDetailHandler>();
        services.AddScoped<GetMyCourseProgressHandler>();
        return services;
    }
}
