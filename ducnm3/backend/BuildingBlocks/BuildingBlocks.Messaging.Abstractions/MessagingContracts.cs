namespace BuildingBlocks.Messaging.Abstractions;

/// <summary>Marker cho message COMMAND có đúng một service owner xử lý qua queue trực tiếp.</summary>
public interface ICommand;

/// <summary>Marker cho integration event được publish để nhiều subscriber nhận độc lập.</summary>
public interface IIntegrationEvent;

/// <summary>Port Application dùng để gửi command mà không phụ thuộc MassTransit hoặc RabbitMQ.</summary>
public interface ICommandSender
{
    /// <summary>Gửi command đến owner service. Input là tên service đích và payload; task hoàn thành khi transport nhận lệnh gửi.</summary>
    Task SendAsync<TCommand>(
        string destinationService,
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand;
}

/// <summary>Port Application dùng để publish event mà không lộ chi tiết transport.</summary>
public interface IEventPublisher
{
    /// <summary>Publish event để các subscriber đã đăng ký nhận; task hoàn thành khi event được đưa cho transport.</summary>
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;
}

/// <summary>Cung cấp correlation ID hiện hành cho transport, lấy từ HTTP request hoặc activity nền.</summary>
public interface ICorrelationContextAccessor
{
    string? CorrelationId { get; }
}
