using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using StudentService.Application.UseCases.Students.GetSummary;

namespace StudentService.Api.Endpoints.Students.GetSummary;

public static class GetStudentsSummaryEndpoint
{
    public static RouteHandlerBuilder MapGetStudentsSummary(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Students.Summary,
                async (HttpContext context, GetStudentsSummaryHandler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Ok(ApiResponseFactory.Success(result, context.TraceIdentifier));
                })
            .WithName("get-students-summary")
            .WithTags(ServiceNames.Student)
            .Produces<ApiResponse<StudentsSummaryResult>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Admin);
}
