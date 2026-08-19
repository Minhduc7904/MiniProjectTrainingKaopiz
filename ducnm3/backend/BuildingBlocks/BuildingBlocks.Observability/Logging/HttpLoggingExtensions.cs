using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace BuildingBlocks.Observability.Logging;

public static class HttpLoggingExtensions
{
    public static WebApplication UseLmsHttpLogging(
        this WebApplication app)
    {
        // Đưa TraceId + CorrelationId vào LogContext
        // để mọi ILogger bên trong request tự động có hai property này.
        app.Use(async (context, next) =>
        {
            var correlationId = context.TraceIdentifier;

            var traceId =
                Activity.Current?.TraceId.ToString()
                ?? context.TraceIdentifier;

            using (LogContext.PushProperty(
                       "CorrelationId",
                       correlationId))
            using (LogContext.PushProperty(
                       "TraceId",
                       traceId))
            {
                await next();
            }
        });

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} " +
                "responded {StatusCode} in {Elapsed:0.000} ms";

            options.GetLevel =
                (httpContext, elapsed, exception) =>
                {
                    // Health check thường được Docker gọi liên tục.
                    if (httpContext.Request.Path
                        .StartsWithSegments("/health"))
                    {
                        return LogEventLevel.Debug;
                    }

                    if (exception is not null ||
                        httpContext.Response.StatusCode >= 500)
                    {
                        return LogEventLevel.Error;
                    }

                    if (httpContext.Response.StatusCode >= 400)
                    {
                        return LogEventLevel.Warning;
                    }

                    return LogEventLevel.Information;
                };

            options.EnrichDiagnosticContext =
                (diagnosticContext, httpContext) =>
                {
                    var correlationId =
                        httpContext.TraceIdentifier;

                    var traceId =
                        Activity.Current?.TraceId.ToString()
                        ?? httpContext.TraceIdentifier;

                    diagnosticContext.Set(
                        "CorrelationId",
                        correlationId);

                    diagnosticContext.Set(
                        "TraceId",
                        traceId);

                    diagnosticContext.Set(
                        "RequestHost",
                        httpContext.Request.Host.Value);

                    diagnosticContext.Set(
                        "RequestScheme",
                        httpContext.Request.Scheme);
                };
        });

        return app;
    }
}