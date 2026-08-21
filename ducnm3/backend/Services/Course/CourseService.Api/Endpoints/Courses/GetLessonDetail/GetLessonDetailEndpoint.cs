using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using CourseService.Api.Contracts.Courses;
using CourseService.Application.Common.Errors;
using CourseService.Application.UseCases.Lessons.GetDetail;
using CourseService.Application.Services.Media;
using CourseService.Api.Mappers;
using CourseService.Application.Services.Content;

namespace CourseService.Api.Endpoints.Courses.GetLessonDetail;

public static class GetLessonDetailEndpoint
{
    public static RouteHandlerBuilder MapGetLessonDetail(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
            ApiRoutes.Courses.LessonByIdTemplate,
            async (string courseId, string lessonId, HttpContext context, GetLessonDetailHandler handler, ICourseMediaReader mediaReader, IMarkdownHtmlRenderer markdownRenderer, CancellationToken cancellationToken) =>
            {
                if (!Guid.TryParse(courseId, out var parsedCourseId) || parsedCourseId == Guid.Empty ||
                    !Guid.TryParse(lessonId, out var parsedLessonId) || parsedLessonId == Guid.Empty)
                {
                    throw CourseErrors.ValidationFailed([]);
                }

                var lesson = await handler.HandleAsync(parsedCourseId, parsedLessonId, cancellationToken);
                var attachments = await mediaReader.GetLessonAttachmentsAsync(parsedLessonId, cancellationToken);
                context.Response.Headers.CacheControl = "no-store";
                return Results.Json(ApiResponseFactory.Success(
                    new LessonCommandResponse(lesson.Id, lesson.CourseId, lesson.Title, lesson.ContentMarkdown, markdownRenderer.Render(lesson.ContentMarkdown), lesson.DisplayOrder, lesson.CreatedAtUtc, lesson.UpdatedAtUtc, attachments.Select(CourseResponseMapper.ToMediaResponse).ToArray()),
                    context.TraceIdentifier));
            })
            .WithName("get-course-lesson-detail")
            .WithTags(ServiceNames.Course)
            .Produces<ApiResponse<LessonCommandResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
