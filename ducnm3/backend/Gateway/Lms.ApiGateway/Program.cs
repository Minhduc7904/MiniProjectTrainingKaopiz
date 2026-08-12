using NSwag.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/swagger";
        settings.SwaggerRoutes.Add(new SwaggerUiRoute("Course Service", "/course/swagger/v1/swagger.json"));
        settings.SwaggerRoutes.Add(new SwaggerUiRoute("Student Service", "/student/swagger/v1/swagger.json"));
        settings.SwaggerRoutes.Add(new SwaggerUiRoute("Media Service", "/media/swagger/v1/swagger.json"));
        settings.SwaggerRoutes.Add(new SwaggerUiRoute("Notification Service", "/notification/swagger/v1/swagger.json"));
    });
}

app.MapGet("/", () => Results.Ok(new { service = "lms-api-gateway", status = "ready" }));
app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();
