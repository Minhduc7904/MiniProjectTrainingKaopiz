using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using StudentService.Api.Contracts.Students;
using StudentService.Api.Mappers;
using StudentService.Application.Common.Errors;
using StudentService.Application.UseCases.Students.GetById;

namespace StudentService.Api.Endpoints.Students.GetById;

public static class GetStudentByIdEndpoint
{
    public static RouteHandlerBuilder MapGetStudentById(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(
                ApiRoutes.Students.GetByIdTemplate,
                async (string studentId, HttpContext context, GetStudentByIdHandler handler, CancellationToken cancellationToken) =>
                {
                    if (!Guid.TryParse(studentId, out var parsedStudentId))
                    {
                        throw StudentErrors.ValidationFailed();
                    }

                    var result = await handler.HandleAsync(
                        new GetStudentByIdQuery(parsedStudentId),
                        cancellationToken);
                    return Results.Json(ApiResponseFactory.Success(StudentResponseMapper.ToResponse(result), context.TraceIdentifier));
                })
            .WithName("get-student-by-id")
            .WithTags(ServiceNames.Student)
            .Produces<ApiResponse<StudentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);
}
