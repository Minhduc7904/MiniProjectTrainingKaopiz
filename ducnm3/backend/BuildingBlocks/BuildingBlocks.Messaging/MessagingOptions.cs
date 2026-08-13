using System.Globalization;

namespace BuildingBlocks.Messaging;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public RabbitMqOptions RabbitMq { get; init; } = new();

    public MessagingRetryOptions Retry { get; init; } = new();

    public MessagingConsumerOptions Consumer { get; init; } = new();

    public MessagingHostOptions Host { get; init; } = new();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(RabbitMq.Host))
        {
            throw new InvalidOperationException("Messaging:RabbitMq:Host is required.");
        }

        if (RabbitMq.Port is < 1 or > 65_535)
        {
            throw new InvalidOperationException("Messaging:RabbitMq:Port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(RabbitMq.VirtualHost))
        {
            throw new InvalidOperationException("Messaging:RabbitMq:VirtualHost is required.");
        }

        if (string.IsNullOrWhiteSpace(RabbitMq.Username) ||
            string.IsNullOrWhiteSpace(RabbitMq.Password))
        {
            throw new InvalidOperationException(
                "Messaging RabbitMQ username and password are required.");
        }

        if (Retry.RetryCount is < 0 or > 20)
        {
            throw new InvalidOperationException(
                "Messaging:Retry:RetryCount must be between 0 and 20.");
        }

        if (Retry.InitialIntervalSeconds <= 0 || Retry.IntervalIncrementSeconds < 0)
        {
            throw new InvalidOperationException(
                "Messaging retry intervals must be positive.");
        }

        if (Consumer.PrefetchCount is < 1 or > ushort.MaxValue)
        {
            throw new InvalidOperationException(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"Messaging:Consumer:PrefetchCount must be between 1 and {ushort.MaxValue}."));
        }

        if (Consumer.ConcurrencyLimit is < 1 or > 1_000)
        {
            throw new InvalidOperationException(
                "Messaging:Consumer:ConcurrencyLimit must be between 1 and 1000.");
        }

        if (Host.StartTimeoutSeconds is < 1 or > 300 ||
            Host.StopTimeoutSeconds is < 1 or > 300)
        {
            throw new InvalidOperationException(
                "Messaging host start/stop timeout must be between 1 and 300 seconds.");
        }
    }
}

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = string.Empty;

    public ushort Port { get; init; } = 5672;

    public string VirtualHost { get; init; } = "/";

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public sealed class MessagingRetryOptions
{
    public int RetryCount { get; init; } = 3;

    public double InitialIntervalSeconds { get; init; } = 1;

    public double IntervalIncrementSeconds { get; init; } = 2;
}

public sealed class MessagingConsumerOptions
{
    public int PrefetchCount { get; init; } = 32;

    public int ConcurrencyLimit { get; init; } = 8;
}

public sealed class MessagingHostOptions
{
    public int StartTimeoutSeconds { get; init; } = 30;

    public int StopTimeoutSeconds { get; init; } = 30;
}
