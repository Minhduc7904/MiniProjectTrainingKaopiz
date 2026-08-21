using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using StudentService.Api.Contracts.Auth;
using StudentService.Api.Contracts.Auth.Register;
using StudentService.Api.Mappers;
using StudentService.Application.UseCases.Auth.Register;

namespace StudentService.Api.Endpoints.Auth.Register;

public static class RegisterStudentEndpoint
{
    public static RouteHandlerBuilder MapRegisterStudent(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.StudentAuth.Register,
                async (RegisterStudentRequest request, HttpContext context, RegisterStudentHandler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new RegisterStudentCommand(request.Email, request.DisplayName),
                        cancellationToken);
                    context.Response.Headers.Location = ApiRoutes.Students.GetByIdPublicPath(result.Id);
                    return Results.Json(
                        ApiResponseFactory.Success(StudentAuthResponseMapper.ToResponse(result), context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("register-student")
            .WithTags(ServiceNames.Student)
            .Accepts<RegisterStudentRequest>("application/json")
            .Produces<ApiResponse<StudentActorResponse>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);
}
