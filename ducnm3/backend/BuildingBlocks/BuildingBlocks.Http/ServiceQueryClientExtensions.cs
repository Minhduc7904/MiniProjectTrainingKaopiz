using BuildingBlocks.Contracts.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace BuildingBlocks.Http;

public static class ServiceQueryClientExtensions
{
    public static IHttpClientBuilder AddServiceQueryClient<TClient, TImplementation>(
        this IServiceCollection services,
        IConfiguration configuration,
        string destinationService)
        where TClient : class
        where TImplementation : class, TClient
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationService);

        var endpoint = configuration[
            $"{ConfigurationSectionNames.ServiceEndpoints}:{destinationService}"];
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseAddress) ||
            baseAddress.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException(
                $"A valid HTTP(S) ServiceEndpoints:{destinationService} value is required.");
        }

        var options = configuration
            .GetSection(ServiceQueryOptions.SectionName)
            .Get<ServiceQueryOptions>() ?? new ServiceQueryOptions();
        options.Validate();

        services.AddSingleton(options);
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdDelegatingHandler>();

        var builder = services
            .AddHttpClient<TClient, TImplementation>(client =>
            {
                client.BaseAddress = baseAddress;
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

        builder.AddStandardResilienceHandler(resilience =>
        {
            resilience.TotalRequestTimeout.Timeout =
                TimeSpan.FromSeconds(options.TimeoutSeconds);
            resilience.AttemptTimeout.Timeout =
                TimeSpan.FromSeconds(options.TimeoutSeconds);
            resilience.Retry.MaxRetryAttempts = options.RetryCount;
            resilience.Retry.Delay =
                TimeSpan.FromMilliseconds(options.RetryDelayMilliseconds);
            resilience.Retry.BackoffType = DelayBackoffType.Exponential;
            resilience.Retry.UseJitter = true;
            resilience.Retry.DisableForUnsafeHttpMethods();
        });

        return builder;
    }
}
