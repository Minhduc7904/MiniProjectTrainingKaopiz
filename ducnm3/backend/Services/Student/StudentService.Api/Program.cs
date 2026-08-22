using BuildingBlocks.Contracts.Api;
using BuildingBlocks.DatabaseMigration;
using BuildingBlocks.Messaging;
using BuildingBlocks.Presentation.Extensions;
using BuildingBlocks.Observability.Logging;
using Microsoft.Extensions.Logging;
using StudentService.Api.Endpoints.Auth.GetMe;
using StudentService.Api.Endpoints.Auth.Login;
using StudentService.Api.Endpoints.Auth.Register;
using StudentService.Api.Endpoints.Students.GetById;
using StudentService.Api.Endpoints.Students.GetList;
using StudentService.Api.Endpoints.Students.GetSummary;
using StudentService.Application;
using StudentService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddLmsSerilog(ServiceNames.Student);
builder.Services.AddHealthChecks();
var migrationsRunOnly = builder.Configuration.GetValue<bool>("Migrations:RunOnly");
if (!migrationsRunOnly)
{
    builder.Services.AddLmsMessaging(builder.Configuration, ServiceNames.Student);
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "Student Service API";
    document.Version = "v1";
});

var connectionString = builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__Database environment variable is required for Student Service.");
}

builder.Services
    .AddStudentApplication()
    .AddStudentInfrastructure(connectionString);

var app = builder.Build();
var logMigration = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1000, "SqlMigration"),
    "{MigrationMessage}");

await SqlMigrationRunner.ApplyAsync(
    new SqlMigrationRunnerOptions(
        ServiceNames.Student,
        connectionString,
        Path.Combine(app.Environment.ContentRootPath, "Database", "Migrations")),
    message => logMigration(app.Logger, message, null));

if (migrationsRunOnly)
{
    return;
}

app.UseSharedApiMiddleware();
app.UseLmsHttpLogging();

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseOpenApi();
    app.UseSwaggerUi(settings => settings.Path = "/swagger");
}

app.MapServiceInfoEndpoint(ServiceNames.Student);
app.MapDatabaseHealthEndpoint(ServiceNames.Student);
app.MapGetStudents();
app.MapGetStudentsSummary();
app.MapGetStudentById();
app.MapRegisterStudent();
app.MapLoginStudent();
app.MapGetCurrentStudent();

app.Run();
