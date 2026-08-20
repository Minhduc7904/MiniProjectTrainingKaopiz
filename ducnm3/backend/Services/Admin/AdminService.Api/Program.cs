using AdminService.Application.Features.Admins.GetById;
using AdminService.Infrastructure;
using BuildingBlocks.Contracts.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<GetAdminByIdHandler>();
builder.Services.AddSingleton<IAdminRepository>(ConfigurationAdminRepositoryFactory.Create(
    [builder.Configuration["Admin:Ids:0"] ?? string.Empty],
    builder.Configuration["Admin:DisplayName"] ?? "Development Admin"));

var app = builder.Build();
app.MapGet(ApiRoutes.Admins.GetByIdTemplate, async (
    string adminId,
    GetAdminByIdHandler handler,
    HttpContext context,
    CancellationToken cancellationToken) =>
{
    if (!Guid.TryParse(adminId, out var id))
    {
        throw new AdminApplicationException(ApiErrorCodes.ValidationFailed, ApiErrorMessages.ValidationFailed, 400);
    }

    try
    {
        var result = await handler.HandleAsync(id, cancellationToken);
        return Results.Json(new { data = result, meta = new { traceId = context.TraceIdentifier } });
    }
    catch (AdminApplicationException exception)
    {
        return Results.Json(new
        {
            error = new { code = exception.ErrorCode, message = exception.Message },
            meta = new { traceId = context.TraceIdentifier },
        }, statusCode: exception.StatusCode);
    }
}).WithName("get-admin-by-id").WithTags(ServiceNames.Admin);
app.Run();

public partial class Program;
