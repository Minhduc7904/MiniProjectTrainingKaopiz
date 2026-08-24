using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using Microsoft.AspNetCore.Mvc;
using StudentService.Api.Contracts.Students;
using StudentService.Api.Mappers;
using StudentService.Application.UseCases.Students.GetList;

namespace StudentService.Api.Endpoints.Students.GetList;

public static class GetStudentsEndpoint
{
    public static RouteHandlerBuilder MapGetStudents(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Students.List,
                async (string? status, string? search, string? sortBy, string? sortDirection, int? page, int? pageSize, HttpContext context, [FromServices] GetStudentsHandler handler, CancellationToken cancellationToken) =>
                {
                    var query = GetStudentsQuery.Create(status, search, sortBy, sortDirection, page, pageSize);
                    var result = await handler.HandleAsync(query, cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(
                        ApiResponseFactory.Success(
                            result.Items.Select(StudentResponseMapper.ToResponse).ToArray(),
                            context.TraceIdentifier,
                            new OffsetPaginationMeta(query.Page, query.PageSize, result.TotalItems, result.TotalPages)));
                })
            .WithName("get-students")
            .WithTags(ServiceNames.Student)
            .Produces<ApiResponse<IReadOnlyList<StudentListItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
}
