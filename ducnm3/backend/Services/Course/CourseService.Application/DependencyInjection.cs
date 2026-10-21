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
using CourseService.Application.UseCases.LessonAi.Documents;
using CourseService.Application.UseCases.LessonAi.Jobs;
using CourseService.Application.UseCases.LessonAi.Processing;
using CourseService.Application.UseCases.Learning;
using CourseService.Application.UseCases.Learning.Quizzes;
using CourseService.Application.UseCases.Quizzes.Create;
using CourseService.Application.UseCases.Quizzes.Delete;
using CourseService.Application.UseCases.Quizzes.GetDetail;
using CourseService.Application.UseCases.Quizzes.GetList;
using CourseService.Application.UseCases.Quizzes.Update;
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
        services.AddScoped<CreateQuizHandler>();
        services.AddScoped<UpdateQuizHandler>();
        services.AddScoped<DeleteQuizHandler>();
        services.AddScoped<GetQuizDetailHandler>();
        services.AddScoped<GetQuizzesHandler>();
        services.AddScoped<StudentQuizAccess>();
        services.AddScoped<GetStudentQuizzesHandler>();
        services.AddScoped<GetStudentQuizDetailHandler>();
        services.AddScoped<StartQuizAttemptHandler>();
        services.AddScoped<SaveQuizAnswersHandler>();
        services.AddScoped<SubmitQuizAttemptHandler>();
        services.AddScoped<GetQuizAttemptsHandler>();
        services.AddScoped<GetQuizAttemptDetailHandler>();
        services.AddScoped<GetLessonAiDocumentsHandler>();
        services.AddScoped<SyncLessonAiDocumentsHandler>();
        services.AddScoped<CreateLessonAiJobHandler>();
        services.AddScoped<GetLessonAiJobsHandler>();
        services.AddScoped<DocumentExtractionProcessor>();
        services.AddScoped<LessonAiAgentLoop>();
        services.AddScoped<LessonAiJobProcessor>();
        services.AddScoped<LessonAiWorkRunner>();
        return services;
    }
}
