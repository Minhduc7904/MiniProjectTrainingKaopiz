using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Students;
using BuildingBlocks.Presentation.Api;
using StudentService.Application.Features.Students.GetById;

namespace StudentService.Api.Endpoints;

public static class StudentEndpoints
{
    public static RouteHandlerBuilder MapGetStudentById(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapGet(
                ApiRoutes.Students.GetByIdTemplate,
                async (
                    string studentId,
                    HttpContext context,
                    GetStudentByIdHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(studentId, out var parsedStudentId))
                    {
                        throw new StudentApplicationException(
                            ApiErrorCodes.ValidationFailed,
                            ApiErrorMessages.ValidationFailed,
                            400);
                    }

                    var student = await handler.HandleAsync(
                        parsedStudentId,
                        cancellationToken);
                    return Results.Json(
                        ApiResponseFactory.Success(
                            student,
                            context.TraceIdentifier));
                })
            .WithName("get-student-by-id")
            .WithTags(ServiceNames.Student)
            .Produces<ApiResponse<StudentQueryResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
