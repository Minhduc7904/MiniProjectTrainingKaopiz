using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging;
using BuildingBlocks.Observability.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.AddLmsSerilog(ServiceNames.Scheduler);
builder.Services.AddHealthChecks();
builder.Services.AddLmsMessaging(builder.Configuration, ServiceNames.Scheduler);

var host = builder.Build();
await host.RunAsync();
