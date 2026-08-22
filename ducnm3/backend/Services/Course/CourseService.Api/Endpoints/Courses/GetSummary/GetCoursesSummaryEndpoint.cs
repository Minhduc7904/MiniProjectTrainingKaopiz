// File: backend/Services/Course/CourseService.Api/Endpoints/Courses/GetSummary/GetCoursesSummaryEndpoint.cs
// Mục đích: Map endpoint đọc tổng số Course và Lesson cho dashboard quản trị.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Application.UseCases.Courses.GetSummary;

namespace CourseService.Api.Endpoints.Courses.GetSummary;

public static class GetCoursesSummaryEndpoint
{
    public static RouteHandlerBuilder MapGetCoursesSummary(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Courses.Summary,
                async (HttpContext context, GetCoursesSummaryHandler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Ok(ApiResponseFactory.Success(result, context.TraceIdentifier));
                })
            .WithName("get-courses-summary")
            .WithTags(ServiceNames.Course)
            .Produces<ApiResponse<CoursesSummaryResult>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Admin);
}
