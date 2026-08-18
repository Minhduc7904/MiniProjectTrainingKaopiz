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
    public NotificationSnapshotWork SnapshotWork { get; set; } = new(false, false);
    public bool SnapshotCompleted { get; set; }
    public bool SnapshotMarkedFailed { get; private set; }

    public Task<NotificationBatchSummary> CreateAsync(
        CreateNotificationBatchRecord record,
        CancellationToken cancellationToken)
    {
        CreatedRecord = record;
        return Task.FromResult(new NotificationBatchSummary(
            record.Id,
            "PENDING",
            0,
            0,
            0,
            0,
            record.BatchSize,
            record.CreatedAtUtc,
            null,
            null));
    }

    public Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult<NotificationBatchSummary?>(null);

    public Task<NotificationBatchFailedItemsPage> GetFailedItemsAsync(
        Guid batchId,
        Guid? afterItemId,
        int limit,
        CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationBatchFailedItemsPage([], null, false));

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<NotificationSnapshotWork> PrepareSnapshotAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(SnapshotWork);

    public Task AppendSnapshotPageAsync(
        Guid batchId,
        IReadOnlyList<Guid> studentIds,
        CancellationToken cancellationToken)
    {
        SnapshotPages.Add(studentIds);
        return Task.CompletedTask;
    }

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

    public Task<bool> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(hasRemaining);
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
