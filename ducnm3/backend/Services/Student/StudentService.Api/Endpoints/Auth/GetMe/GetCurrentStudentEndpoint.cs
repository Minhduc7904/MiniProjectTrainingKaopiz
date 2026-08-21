using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using StudentService.Api.Contracts.Auth.GetMe;
using StudentService.Api.Mappers;
using StudentService.Application.UseCases.Auth.GetMe;

namespace StudentService.Api.Endpoints.Auth.GetMe;

public static class GetCurrentStudentEndpoint
{
    public static RouteHandlerBuilder MapGetCurrentStudent(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.StudentAuth.Me,
                async (HttpContext context, GetCurrentStudentHandler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new GetCurrentStudentQuery(context.GetRequiredActor().Id),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(ApiResponseFactory.Success(StudentAuthResponseMapper.ToResponse(result), context.TraceIdentifier));
                })
            .WithName("get-current-student")
            .WithTags(ServiceNames.Student)
            .Produces<ApiResponse<GetCurrentStudentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireActor(ActorAccess.Student);
}
