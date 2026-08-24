using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.Learning;
using CourseService.Api.Contracts.Courses;
using CourseService.Api.Mappers;
using CourseService.Application.Common.Errors;
using CourseService.Application.Services.Content;
using CourseService.Application.Services.Media;
using CourseService.Application.UseCases.Learning;
using Microsoft.AspNetCore.Mvc;

namespace CourseService.Api.Endpoints.Learning;

public static class LearningEndpoints
{
    public static RouteHandlerBuilder MapGetStudentCourseCatalog(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.StudentCourseCatalog, async (string? search, int? page, int? pageSize, HttpContext context, [FromServices] GetStudentCourseCatalogHandler handler, ICourseMediaReader mediaReader, CancellationToken cancellationToken) =>
        {
            var query = GetStudentCourseCatalogQuery.Create(search, page, pageSize);
            var result = await handler.HandleAsync(context.GetRequiredActor().Id, query, cancellationToken);
            var media = await mediaReader.GetManyAsync(result.Items.Select(item => item.CourseId).ToArray(), cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(
                StudentLearningResponseMapper.ToCatalogResponses(result.Items, media),
                context.TraceIdentifier,
                new OffsetPaginationMeta(query.Page, query.PageSize, result.TotalItems, result.TotalPages)));
        }).WithName("get-student-course-catalog").WithTags(ServiceNames.Course)
          .Produces<ApiResponse<IReadOnlyList<StudentCourseCatalogResponse>>>(StatusCodes.Status200OK)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapGetStudentEnrollments(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.StudentEnrollments, async (int? page, int? pageSize, HttpContext context, [FromServices] GetStudentEnrollmentsHandler handler, ICourseMediaReader mediaReader, CancellationToken cancellationToken) =>
        {
            var query = GetStudentEnrollmentsQuery.Create(page, pageSize);
            var result = await handler.HandleAsync(context.GetRequiredActor().Id, query, cancellationToken);
            var media = await mediaReader.GetManyAsync(result.Items.Select(item => item.CourseId).ToArray(), cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(
                StudentLearningResponseMapper.ToEnrollmentResponses(result.Items, media),
                context.TraceIdentifier,
                new OffsetPaginationMeta(query.Page, query.PageSize, result.TotalItems, result.TotalPages)));
        }).WithName("get-student-enrollments").WithTags(ServiceNames.Course)
          .Produces<ApiResponse<IReadOnlyList<StudentEnrollmentResponse>>>(StatusCodes.Status200OK)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapGetStudentEnrollmentDetail(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.StudentEnrollmentDetailTemplate, async (string courseId, HttpContext context, GetStudentEnrollmentDetailHandler handler, ICourseMediaReader mediaReader, IMarkdownHtmlRenderer markdownRenderer, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId) || parsedCourseId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
            var result = await handler.HandleAsync(parsedCourseId, context.GetRequiredActor().Id, cancellationToken);
            var media = await mediaReader.GetAsync(parsedCourseId, cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(StudentLearningResponseMapper.ToDetailResponse(result, media, markdownRenderer), context.TraceIdentifier));
        }).WithName("get-student-enrollment-detail").WithTags(ServiceNames.Course)
          .Produces<ApiResponse<StudentEnrollmentDetailResponse>>(StatusCodes.Status200OK)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
          .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapGetStudentLessonDetail(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.StudentLessonDetailTemplate, async (string courseId, string lessonId, HttpContext context, GetStudentLessonDetailHandler handler, ICourseMediaReader mediaReader, IMarkdownHtmlRenderer markdownRenderer, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId) || parsedCourseId == Guid.Empty || !Guid.TryParse(lessonId, out var parsedLessonId) || parsedLessonId == Guid.Empty)
            {
                throw CourseErrors.ValidationFailed([]);
            }

            var lesson = await handler.HandleAsync(parsedCourseId, parsedLessonId, context.GetRequiredActor().Id, cancellationToken);
            var attachments = await mediaReader.GetLessonAttachmentsAsync(parsedLessonId, cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(
                new LessonCommandResponse(lesson.Id, lesson.CourseId, lesson.Title, lesson.ContentMarkdown, markdownRenderer.Render(lesson.ContentMarkdown), lesson.DisplayOrder, lesson.CreatedAtUtc, lesson.UpdatedAtUtc, attachments.Select(CourseResponseMapper.ToMediaResponse).ToArray()),
                context.TraceIdentifier));
        }).WithName("get-student-lesson-detail").WithTags(ServiceNames.Course)
          .Produces<ApiResponse<LessonCommandResponse>>(StatusCodes.Status200OK)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
          .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapGetMyCourseProgress(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.MyProgressTemplate, async (string courseId, HttpContext context, GetMyCourseProgressHandler handler, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId) || parsedCourseId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
            var result = await handler.HandleAsync(parsedCourseId, context.GetRequiredActor().Id, cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(StudentLearningResponseMapper.ToProgressResponse(result), context.TraceIdentifier));
        }).WithName("get-my-course-progress").WithTags(ServiceNames.Course)
          .Produces<ApiResponse<StudentCourseProgressResponse>>(StatusCodes.Status200OK)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
          .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapEnrollCourse(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Courses.EnrollmentsTemplate, async (string courseId, HttpContext context, EnrollCourseHandler handler, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId)) throw CourseErrors.ValidationFailed([]);
            var result = await handler.HandleAsync(parsedCourseId, context.GetRequiredActor().Id, cancellationToken);
            return Results.Json(ApiResponseFactory.Success(result, context.TraceIdentifier), statusCode: StatusCodes.Status201Created);
        }).WithName("enroll-course").WithTags(ServiceNames.Course)
          .Produces<ApiResponse<EnrollmentResult>>(StatusCodes.Status201Created)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
          .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
          .RequireActor(ActorAccess.Student);

    public static RouteHandlerBuilder MapCompleteLessonProgress(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(ApiRoutes.Courses.CompleteLessonProgressTemplate, async (string courseId, string lessonId, HttpContext context, CompleteLessonProgressHandler handler, CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(courseId, out var parsedCourseId) || !Guid.TryParse(lessonId, out var parsedLessonId)) throw CourseErrors.ValidationFailed([]);
            var result = await handler.HandleAsync(parsedCourseId, parsedLessonId, context.GetRequiredActor().Id, cancellationToken);
            return Results.Json(ApiResponseFactory.Success(result, context.TraceIdentifier));
        }).WithName("complete-lesson-progress").WithTags(ServiceNames.Course)
          .Produces<ApiResponse<LessonProgressResult>>(StatusCodes.Status200OK)
          .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
          .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
          .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
          .RequireActor(ActorAccess.Student);
}
