// File: backend/Services/Course/CourseService.Api/Endpoints/Courses/GetList/GetCoursesEndpoint.cs
// Mục đích: Map HTTP GET list Course, chuyển use case result sang API contract và đặt no-store.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using CourseService.Api.Contracts.Courses;
using CourseService.Api.Mappers;
using CourseService.Application.UseCases.Courses.GetList;
using Microsoft.AspNetCore.Mvc;

namespace CourseService.Api.Endpoints.Courses.GetList;

public static class GetCoursesEndpoint
{
    public static RouteHandlerBuilder MapGetCourses(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.Courses.List, async (string? status, string? sortBy, string? sortDirection, int? page, int? pageSize, HttpContext context, [FromServices] GetCoursesHandler handler, CancellationToken cancellationToken) =>
        {
            var query = GetCoursesQuery.Create(status, sortBy, sortDirection, page, pageSize);
            var result = await handler.HandleAsync(query, cancellationToken);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(ApiResponseFactory.Success(CourseResponseMapper.ToListResponse(result.Items), context.TraceIdentifier, new OffsetPaginationMeta(query.Page, query.PageSize, result.TotalItems, result.TotalPages)));
        })
        .WithName("get-courses")
        .WithTags(ServiceNames.Course)
        .Produces<ApiResponse<IReadOnlyList<CourseListItemResponse>>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
}
