using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using StudentService.Application.Students;

namespace StudentService.Api.Endpoints;

public static class StudentEndpoints
{
    public static RouteHandlerBuilder MapGetStudentById(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapGet(
                "/api/students/{studentId}",
                async (
                    string studentId,
                    HttpContext context,
                    GetStudentByIdHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(studentId, out var parsedStudentId))
                    {
                        throw new StudentApplicationException(
                            "VALIDATION_ERROR",
                            "studentId must be a valid UUID.",
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
            .Produces<ApiResponse<StudentDetails>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
