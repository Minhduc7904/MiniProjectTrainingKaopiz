// File: backend/Services/Notification/NotificationService.UnitTests/Application/UseCases/NotificationBatches/TestDoubles/NotificationBatchTestDoubles.cs
// Mục đích: Cung cấp test double dùng chung để kiểm thử độc lập các use case tạo, snapshot và dispatch Notification Batch.

using System.Runtime.CompilerServices;
using BuildingBlocks.Messaging.Abstractions;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Services.Sending;
using NotificationService.Application.Services.Students;

namespace NotificationService.UnitTests.Application.UseCases.NotificationBatches.TestDoubles;

internal sealed class StubStudentRecipientClient(params IReadOnlyList<Guid>[] pages) : IStudentRecipientClient
{
    public async IAsyncEnumerable<IReadOnlyList<Guid>> GetActiveStudentIdPagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var page in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return page;
        }
    }
}

internal sealed class FailingStudentRecipientClient : IStudentRecipientClient
{
    public async IAsyncEnumerable<IReadOnlyList<Guid>> GetActiveStudentIdPagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        if (cancellationToken.IsCancellationRequested)
        {
            yield break;
        }
        throw NotificationErrors.StudentServiceUnavailable();
    }
}

internal sealed class StubBatchRepository :
    INotificationBatchRepository,
    INotificationBatchDispatchRepository
{
    private readonly IReadOnlyList<NotificationBatchWorkItem>? items;
    private readonly bool hasRemaining;

    public StubBatchRepository(IReadOnlyList<NotificationBatchWorkItem>? items = null, bool hasRemaining = false)
    {
        this.items = items;
        this.hasRemaining = hasRemaining;
    }

    public CreateNotificationBatchRecord? CreatedRecord { get; private set; }
    public List<Guid> FailedItems { get; } = [];
    public List<Guid> SuccessItems { get; } = [];
    public List<IReadOnlyList<Guid>> SnapshotPages { get; } = [];
    public Queue<int> SnapshotInsertedCounts { get; } = [];
    public NotificationSnapshotWork SnapshotWork { get; set; } = new(false, false);
    public bool SnapshotCompleted { get; set; }
    public bool SnapshotMarkedFailed { get; private set; }
    public NotificationBatchSummary? BatchSummary { get; set; }
    public NotificationBatchSnapshotProgress? SnapshotProgress { get; set; }
    public NotificationBatchListPage? BatchListPage { get; set; }
    public NotificationBatchSummary? RetrySummary { get; set; }
    public bool RetryIsNew { get; set; } = true;

    public Task<NotificationBatchSummary> CreateAsync(
        CreateNotificationBatchRecord record,
        CancellationToken cancellationToken)
    {
        CreatedRecord = record;
        return Task.FromResult(new NotificationBatchSummary(
            record.Id,
            record.Title,
            "PENDING",
            0,
            0,
            0,
            0,
            record.BatchSize,
            record.RequestedCount,
            record.SourceBatchId,
            record.CreatedAtUtc,
            null,
            null));
    }

    public Task<NotificationBatchRetryCreation> PrepareRetryAsync(
        Guid sourceBatchId,
        Guid createdBy,
        DateTime createdAtUtc,
        CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationBatchRetryCreation(
            RetrySummary ?? new NotificationBatchSummary(
                Guid.NewGuid(), "Retry", "PENDING", 0, 0, 0, 0, 500,
                1, sourceBatchId, createdAtUtc, null, null),
            RetryIsNew));

    public Task<NotificationBatchSummary> CommitRetryAsync(
        Guid sourceBatchId,
        CancellationToken cancellationToken) =>
        Task.FromResult(RetrySummary!);

    public Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(BatchSummary?.Id == batchId ? BatchSummary : null);

    public Task<NotificationBatchSnapshotProgress?> GetSnapshotProgressAsync(
        Guid batchId,
        CancellationToken cancellationToken) =>
        Task.FromResult(SnapshotProgress?.BatchId == batchId ? SnapshotProgress : null);

    public Task<NotificationBatchFailedItemsPage> GetFailedItemsAsync(
        Guid batchId,
        Guid? afterItemId,
        int limit,
        CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationBatchFailedItemsPage([], null, false));

    public Task<NotificationBatchListPage> ListAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        Task.FromResult(BatchListPage ?? new NotificationBatchListPage([], page, pageSize, 0));

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<NotificationSnapshotWork> PrepareSnapshotAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(SnapshotWork);

    public Task<int> AppendSnapshotPageAsync(
        Guid batchId,
        IReadOnlyList<Guid> studentIds,
        CancellationToken cancellationToken)
    {
        SnapshotPages.Add(studentIds);
        return Task.FromResult(
            SnapshotInsertedCounts.TryDequeue(out var insertedCount)
                ? insertedCount
                : studentIds.Count);
    }

    public Task CopyFailedRecipientsAsync(
        Guid batchId,
        Guid sourceBatchId,
        CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<bool> CompleteSnapshotAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(SnapshotCompleted);

    public Task MarkSnapshotFailedAsync(Guid batchId, CancellationToken cancellationToken)
    {
        SnapshotMarkedFailed = true;
        return Task.CompletedTask;
    }

    public Task<NotificationBatchClaim?> ClaimChunkAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult<NotificationBatchClaim?>(
            items is null
                ? null
                : new NotificationBatchClaim(batchId, Guid.NewGuid(), items));

    public Task<IReadOnlyList<NotificationSummary>> CompleteClaimAsync(
        NotificationBatchClaim claim,
        IReadOnlyList<NotificationBatchDeliveryResult> results,
        CancellationToken cancellationToken)
    {
        var itemsById = claim.Items.ToDictionary(item => item.Id);
        var notifications = new List<NotificationSummary>();
        foreach (var result in results)
        {
            var item = itemsById[result.ItemId];
            if (!result.IsSuccess)
            {
                FailedItems.Add(item.Id);
                continue;
            }

            SuccessItems.Add(item.Id);
            notifications.Add(new NotificationSummary(
                Guid.NewGuid(),
                item.StudentId,
                item.Title,
                item.BodyMarkdown,
                "BULK",
                "UNREAD",
                item.CreatedBy,
                DateTime.UnixEpoch,
                null));
        }

        return Task.FromResult<IReadOnlyList<NotificationSummary>>(notifications);
    }

    public Task<NotificationBatchContinuation> FinalizeOrHasRemainingAsync(
        Guid batchId,
        CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationBatchContinuation(
            hasRemaining,
            !hasRemaining,
            checked((uint)SuccessItems.Count),
            items is { Count: > 0 } ? items[0].BodyMarkdown : "Body"));
}

internal sealed class StubCommandSender : ICommandSender
{
    public List<ICommand> Commands { get; } = [];

    public Task SendAsync<TCommand>(
        string destinationService,
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
    {
        Commands.Add(command);
        return Task.CompletedTask;
    }
}

internal sealed class ThrowingSender : INotificationSender
{
    public Task SendAsync(Guid studentId, int attempt, CancellationToken cancellationToken) =>
        Task.FromException(new InvalidOperationException("Expected sender failure."));
}

internal sealed class SuccessfulSender : INotificationSender
{
    public Task SendAsync(Guid studentId, int attempt, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
