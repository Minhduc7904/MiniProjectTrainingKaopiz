using System.Globalization;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Observability.Logging;

public static class LmsLoggingExtensions
{
    public static IHostApplicationBuilder AddLmsSerilog(
        this IHostApplicationBuilder builder,
        string serviceName)
    {
        builder.Services.AddSerilog((services, configuration) =>
        {
            configuration
                .MinimumLevel.Information()

                // Framework noise
                .MinimumLevel.Override(
                    "Microsoft",
                    LogEventLevel.Warning)

                // Vẫn muốn thấy service start/stop
                .MinimumLevel.Override(
                    "Microsoft.Hosting.Lifetime",
                    LogEventLevel.Information)

                // Không spam SELECT/INSERT/DELETE bình thường
                .MinimumLevel.Override(
                    "Microsoft.EntityFrameworkCore.Database.Command",
                    LogEventLevel.Warning)

                // Không spam Proxying to / Received response
                .MinimumLevel.Override(
                    "Yarp.ReverseProxy",
                    LogEventLevel.Warning)

                // Tạm thời giữ để học RabbitMQ / MassTransit
                .MinimumLevel.Override(
                    "MassTransit",
                    LogEventLevel.Information)

                .Enrich.FromLogContext()

                .Enrich.WithProperty(
                    "Service",
                    serviceName)

                .Enrich.WithProperty(
                    "Environment",
                    builder.Environment.EnvironmentName)

                .WriteTo.Console(
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate:
                        "[{Timestamp:HH:mm:ss.fff} {Level:u3}] " +
                        "[{Service}] " +
                        "[TraceId={TraceId}] " +
                        "[CorrelationId={CorrelationId}] " +
                        "[MessageId={MessageId}] " +
                        "{Message:lj}" +
                        "{NewLine}{Exception}");

            var seqServerUrl = builder.Configuration[
                "Observability:Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqServerUrl))
            {
                configuration.WriteTo.Seq(
                    seqServerUrl,
                    formatProvider: CultureInfo.InvariantCulture);
            }
        });

        return builder;
    }
}
