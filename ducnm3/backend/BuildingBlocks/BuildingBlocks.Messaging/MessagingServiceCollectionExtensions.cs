using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Contracts.Health;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddLmsMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName) =>
        AddLmsMessagingCore(services, configuration, serviceName, null);

    public static IServiceCollection AddLmsMessagingWithConsumers(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<IBusRegistrationConfigurator> configureConsumers)
    {
        ArgumentNullException.ThrowIfNull(configureConsumers);
        return AddLmsMessagingCore(
            services,
            configuration,
            serviceName,
            configureConsumers);
    }

    private static IServiceCollection AddLmsMessagingCore(
        IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<IBusRegistrationConfigurator>? configureConsumers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var options = configuration
            .GetSection(MessagingOptions.SectionName)
            .Get<MessagingOptions>() ??
            throw new InvalidOperationException(
                $"Configuration section '{MessagingOptions.SectionName}' is required.");
        options.Validate();

        services.AddSingleton(options);
        services.AddSingleton(new MessagingIdentity(serviceName));
        services.AddHttpContextAccessor();
        services.TryAddSingleton<ICorrelationContextAccessor, DefaultCorrelationContextAccessor>();
        services.AddScoped<ICommandSender, MassTransitCommandSender>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        services.AddSingleton<IMessagingHealthProbe, MassTransitMessagingHealthProbe>();
        services.Configure<MassTransitHostOptions>(host =>
        {
            host.WaitUntilStarted = true;
            host.StartTimeout = TimeSpan.FromSeconds(options.Host.StartTimeoutSeconds);
            host.StopTimeout = TimeSpan.FromSeconds(options.Host.StopTimeoutSeconds);
        });

        services.AddMassTransit(registration =>
        {
            registration.SetKebabCaseEndpointNameFormatter();
            configureConsumers?.Invoke(registration);
            registration.AddConfigureEndpointsCallback((_, _, endpoint) =>
            {
                endpoint.ConcurrentMessageLimit = options.Consumer.ConcurrencyLimit;
                endpoint.UseMessageRetry(retry => retry.Incremental(
                    options.Retry.RetryCount,
                    TimeSpan.FromSeconds(options.Retry.InitialIntervalSeconds),
                    TimeSpan.FromSeconds(options.Retry.IntervalIncrementSeconds)));

                if (endpoint is IRabbitMqReceiveEndpointConfigurator rabbitMqEndpoint)
                {
                    rabbitMqEndpoint.PrefetchCount = (ushort)options.Consumer.PrefetchCount;
                }
            });

            registration.UsingRabbitMq((context, rabbitMq) =>
            {
                rabbitMq.Host(
                    options.RabbitMq.Host,
                    options.RabbitMq.Port,
                    options.RabbitMq.VirtualHost,
                    host =>
                    {
                        host.Username(options.RabbitMq.Username);
                        host.Password(options.RabbitMq.Password);
                    });
                rabbitMq.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
