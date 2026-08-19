using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Messaging;

/// <summary>
/// Composition root cho RabbitMQ/MassTransit dùng chung: bind options, đăng ký ports và áp endpoint policy tập trung.
/// Service gọi một overload từ <c>Program.cs</c>; dùng overload consumer khi cần đăng ký handler qua callback.
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>Đăng ký messaging foundation cho service chưa có consumer.</summary>
    public static IServiceCollection AddLmsMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName) =>
        AddLmsMessagingCore(services, configuration, serviceName, null);

    /// <summary>Đăng ký messaging foundation và cho caller tùy biến MassTransit registration.</summary>
    public static IServiceCollection AddLmsMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<IBusRegistrationConfigurator> configureRegistration)
    {
        ArgumentNullException.ThrowIfNull(configureRegistration);
        return AddLmsMessagingCore(
            services,
            configuration,
            serviceName,
            configureRegistration);
    }

    /// <summary>Đăng ký messaging foundation và callback chuyên dùng để thêm command/event consumer.</summary>
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

    /// <summary>Triển khai chung: validate config, đăng ký dependency và cấu hình RabbitMQ topology.</summary>
    private static IServiceCollection AddLmsMessagingCore(
        IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<IBusRegistrationConfigurator>? configureConsumers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        // Bind và validate trước khi add transport để lỗi cấu hình được phát hiện ngay khi service khởi động.
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
            // Retry, concurrency và prefetch được đặt một nơi để consumer không có policy lệch nhau.
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
