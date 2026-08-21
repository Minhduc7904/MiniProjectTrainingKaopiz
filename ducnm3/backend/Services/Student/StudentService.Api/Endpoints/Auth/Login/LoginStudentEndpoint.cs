using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using StudentService.Api.Contracts.Auth;
using StudentService.Api.Contracts.Auth.Login;
using StudentService.Api.Mappers;
using StudentService.Application.Common.Errors;
using StudentService.Application.UseCases.Auth.Login;

namespace StudentService.Api.Endpoints.Auth.Login;

public static class LoginStudentEndpoint
{
    public static RouteHandlerBuilder MapLoginStudent(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.StudentAuth.Login,
                async (LoginStudentRequest request, HttpContext context, LoginStudentHandler handler, CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(request.Id, out var studentId))
                    {
                        throw StudentErrors.ValidationFailed();
                    }

                    var result = await handler.HandleAsync(
                        new LoginStudentCommand(studentId),
                        cancellationToken);
                    return Results.Json(ApiResponseFactory.Success(StudentAuthResponseMapper.ToResponse(result), context.TraceIdentifier));
                })
            .WithName("login-student")
            .WithTags(ServiceNames.Student)
            .Accepts<LoginStudentRequest>("application/json")
            .Produces<ApiResponse<StudentActorResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
