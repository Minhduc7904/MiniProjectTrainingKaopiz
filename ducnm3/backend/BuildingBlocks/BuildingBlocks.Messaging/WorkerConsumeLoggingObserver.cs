using BuildingBlocks.Contracts.Api;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging;

/// <summary>
/// Ghi nhận lúc Worker nhận, hoàn tất hoặc lỗi cuối cùng của mọi message mà không lộ payload hay dữ liệu ký của storage.
/// </summary>
public sealed partial class WorkerConsumeLoggingObserver(
    ILogger<WorkerConsumeLoggingObserver> logger,
    MessagingIdentity identity,
    MessagingOptions options) : IConsumeObserver
{
    /// <summary>Ghi log ngay trước khi MassTransit chuyển message vào consumer.</summary>
    public Task PreConsume<T>(ConsumeContext<T> context)
        where T : class
    {
        var metadata = WorkerMessageLogMetadata.Create(context, identity.ServiceName, options);
        LogMessageReceived(logger, metadata.SourceService, metadata.MessageType, metadata.Queue,
            metadata.MessageId, metadata.CorrelationId, metadata.ConversationId,
            metadata.RetryAttempt, metadata.RedeliveryCount, metadata.RetryLimit);
        return Task.CompletedTask;
    }

    /// <summary>Ghi log khi toàn bộ consumer cho message kết thúc thành công.</summary>
    public Task PostConsume<T>(ConsumeContext<T> context)
        where T : class
    {
        var metadata = WorkerMessageLogMetadata.Create(context, identity.ServiceName, options);
        LogMessageCompleted(logger, metadata.SourceService, metadata.MessageType, metadata.Queue,
            metadata.MessageId, metadata.CorrelationId, metadata.ConversationId,
            metadata.RetryAttempt, metadata.RedeliveryCount, metadata.RetryLimit);
        return Task.CompletedTask;
    }

    /// <summary>Ghi exception khi retry policy đã dừng retry và message được xem là lỗi cuối cùng.</summary>
    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception)
        where T : class
    {
        var metadata = WorkerMessageLogMetadata.Create(context, identity.ServiceName, options);
        LogMessageFailed(logger, exception, metadata.SourceService, metadata.MessageType, metadata.Queue,
            metadata.MessageId, metadata.CorrelationId, metadata.ConversationId,
            metadata.RetryAttempt, metadata.RedeliveryCount, metadata.RetryLimit);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Worker message received. {WorkerEvent} {SourceService} {MessageType} from {Queue}; message {MessageId}, correlation {CorrelationId}, conversation {ConversationId}, retry {RetryAttempt}/{RetryLimit}, redelivery {RedeliveryCount}.")]
    private static partial void LogMessageReceived(
        ILogger logger,
        string sourceService,
        string messageType,
        string queue,
        Guid? messageId,
        string? correlationId,
        Guid? conversationId,
        int retryAttempt,
        int redeliveryCount,
        int retryLimit,
        string workerEvent = "Received");

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Worker message completed. {WorkerEvent} {SourceService} {MessageType} from {Queue}; message {MessageId}, correlation {CorrelationId}, conversation {ConversationId}, retry {RetryAttempt}/{RetryLimit}, redelivery {RedeliveryCount}.")]
    private static partial void LogMessageCompleted(
        ILogger logger,
        string sourceService,
        string messageType,
        string queue,
        Guid? messageId,
        string? correlationId,
        Guid? conversationId,
        int retryAttempt,
        int redeliveryCount,
        int retryLimit,
        string workerEvent = "Completed");

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Worker message failed. {WorkerEvent} {SourceService} {MessageType} from {Queue}; message {MessageId}, correlation {CorrelationId}, conversation {ConversationId}, retry {RetryAttempt}/{RetryLimit}, redelivery {RedeliveryCount}.")]
    private static partial void LogMessageFailed(
        ILogger logger,
        Exception exception,
        string sourceService,
        string messageType,
        string queue,
        Guid? messageId,
        string? correlationId,
        Guid? conversationId,
        int retryAttempt,
        int redeliveryCount,
        int retryLimit,
        string workerEvent = "Failed");
}

/// <summary>Ghi exception cho từng attempt consumer; retry filter ở ngoài sẽ quyết định có thử lại hay đưa message đến lỗi cuối.</summary>
public sealed partial class WorkerAttemptLoggingFilter<T>(
    ILogger<WorkerAttemptLoggingFilter<T>> logger,
    MessagingIdentity identity,
    MessagingOptions options) : IFilter<ConsumeContext<T>>
    where T : class
{
    /// <summary>Chuyển message đi tiếp và ghi lỗi của từng attempt trước khi ném lại cho retry policy.</summary>
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        try
        {
            await next.Send(context);
        }
        catch (Exception exception)
        {
            var metadata = WorkerMessageLogMetadata.Create(context, identity.ServiceName, options);
            LogMessageAttemptFailed(
                logger,
                exception,
                metadata.SourceService,
                metadata.MessageType,
                metadata.Queue,
                metadata.MessageId,
                metadata.CorrelationId,
                metadata.ConversationId,
                metadata.RetryAttempt,
                metadata.RedeliveryCount,
                metadata.RetryLimit);
            throw;
        }
    }

    /// <summary>Đưa filter vào MassTransit probe để topology runtime có thể mô tả middleware này.</summary>
    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("workerAttemptLogging");

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Worker message attempt failed. {WorkerEvent} {SourceService} {MessageType} from {Queue}; message {MessageId}, correlation {CorrelationId}, conversation {ConversationId}, retry {RetryAttempt}/{RetryLimit}, redelivery {RedeliveryCount}.")]
    private static partial void LogMessageAttemptFailed(
        ILogger logger,
        Exception exception,
        string sourceService,
        string messageType,
        string queue,
        Guid? messageId,
        string? correlationId,
        Guid? conversationId,
        int retryAttempt,
        int redeliveryCount,
        int retryLimit,
        string workerEvent = "AttemptFailed");
}

/// <summary>Metadata an toàn, dùng chung cho consume observer và filter để các event log cùng format.</summary>
internal sealed record WorkerMessageLogMetadata(
    string SourceService,
    string MessageType,
    string Queue,
    Guid? MessageId,
    string? CorrelationId,
    Guid? ConversationId,
    int RetryAttempt,
    int RedeliveryCount,
    int RetryLimit)
{
    public static WorkerMessageLogMetadata Create<T>(
        ConsumeContext<T> context,
        string workerService,
        MessagingOptions options)
        where T : class
    {
        var sourceService = GetHeader(context, MessagingHeaderNames.SourceService) ?? workerService;
        var correlationId = GetHeader(context, ApiHeaderNames.CorrelationId) ??
            context.CorrelationId?.ToString();

        return new WorkerMessageLogMetadata(
            sourceService,
            typeof(T).Name,
            context.ReceiveContext.InputAddress.ToString(),
            context.MessageId,
            correlationId,
            context.ConversationId,
            context.GetRetryAttempt(),
            context.GetRedeliveryCount(),
            options.Retry.RetryCount);
    }

    private static string? GetHeader(ConsumeContext context, string key) =>
        context.Headers.TryGetHeader(key, out var value) && value is not null
            ? value.ToString()
            : null;
}
