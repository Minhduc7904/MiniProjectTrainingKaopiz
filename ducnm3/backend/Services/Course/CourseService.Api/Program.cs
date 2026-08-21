// File: backend/Services/Course/CourseService.Api/Program.cs
// Mục đích: Composition root khởi tạo host, đăng ký dependency và map transport của service.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.DatabaseMigration;
using BuildingBlocks.Messaging;
using BuildingBlocks.Presentation.Extensions;
using BuildingBlocks.Observability.Logging;
using CourseService.Api.Endpoints.Courses.Export;
using CourseService.Api.Endpoints.Courses.GetDetails;
using CourseService.Api.Endpoints.Courses.GetList;
using CourseService.Api.Endpoints.Courses.CreateLesson;
using CourseService.Api.Endpoints.Courses.CreateCourse;
using CourseService.Api.Endpoints.Courses.DeleteCourse;
using CourseService.Api.Endpoints.Courses.UpdateCourse;
using CourseService.Api.Endpoints.Courses.UpdateLesson;
using CourseService.Api.Endpoints.Courses.GetLessonDetail;
using CourseService.Api.Endpoints.Courses.ReorderLessons;
using CourseService.Api.Endpoints.Performance;
using CourseService.Application;
using CourseService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddLmsSerilog(ServiceNames.Course);
builder.Services.AddHealthChecks();
var migrationsRunOnly = builder.Configuration.GetValue<bool>("Migrations:RunOnly");
if (!migrationsRunOnly)
{
    builder.Services.AddLmsMessaging(builder.Configuration, ServiceNames.Course);
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "Course Service API";
    document.Version = "v1";
});

var connectionString = builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__Database environment variable is required for Course Service.");
}

if (!migrationsRunOnly)
{
    builder.Services.AddCourseApplication();
    builder.Services.AddCourseInfrastructure(builder.Configuration, connectionString);
}

var app = builder.Build();
var logMigration = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1000, "SqlMigration"),
    "{MigrationMessage}");

await SqlMigrationRunner.ApplyAsync(
    new SqlMigrationRunnerOptions(
        ServiceNames.Course,
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

app.MapServiceInfoEndpoint(ServiceNames.Course);
app.MapDatabaseHealthEndpoint(ServiceNames.Course);
app.MapGetCourses();
app.MapGetCourseDetails();
app.MapCreateLesson();
app.MapCreateCourse();
app.MapUpdateCourse();
app.MapDeleteCourse();
app.MapUpdateLesson();
app.MapGetLessonDetail();
app.MapReorderLessons();
app.MapExportCourses();
if (app.Environment.IsDevelopment())
{
    app.MapBufferedCourseExportBenchmark();
}

app.Run();
