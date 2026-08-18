// File: backend/Services/Notification/NotificationService.UnitTests/Application/UseCases/NotificationBatches/Dispatch/DispatchNotificationBatchHandlerTests.cs
// Mục đích: Kiểm thử dispatch theo chunk, đăng ký Media Usage và retry khi sender lỗi của Notification Batch.

#pragma warning disable CA1707

using MediaService.Contracts.Messaging;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Services.Content;
using NotificationService.Application.UseCases.NotificationBatches;
using NotificationService.Application.UseCases.NotificationBatches.Dispatch;
using NotificationService.UnitTests.Application.UseCases.NotificationBatches.TestDoubles;

namespace NotificationService.UnitTests.Application.UseCases.NotificationBatches.Dispatch;

public sealed class DispatchNotificationBatchHandlerTests
{
    [Test]
    public async Task HandleAsyncSuccessfulChunkQueuesOneMediaUsageBatchCommand()
    {
        var batchId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var createdBy = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var mediaId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var firstItem = new NotificationBatchWorkItem(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            batchId,
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            0,
            "Title",
            $"![media](/media/api/media/{mediaId:D}/content)",
            createdBy);
        var secondItem = firstItem with
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            StudentId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
        };
        var repository = new StubBatchRepository([firstItem, secondItem]);
        var commandSender = new StubCommandSender();
        var handler = new DispatchNotificationBatchHandler(
            repository,
            new SuccessfulSender(),
            commandSender,
            new NotificationMediaReferenceExtractor(),
            new NotificationBatchProcessingOptions());

        await handler.HandleAsync(new DispatchNotificationBatchV1(batchId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.SuccessItems, Has.Count.EqualTo(2));
            Assert.That(commandSender.Commands, Has.One.TypeOf<RegisterNotificationMediaUsageBatchV1>());
            var command = (RegisterNotificationMediaUsageBatchV1)commandSender.Commands[0];
            Assert.That(command.NotificationIds, Has.Count.EqualTo(2));
            Assert.That(command.References, Has.One.EqualTo(
                new NotificationMediaUsageReferenceV1(mediaId, NotificationMediaUsageTypes.Embed, 0)));
        });
    }

    [Test]
    public async Task HandleAsync_FirstBusinessFailureMarksItemForRetryAndRequeues()
    {
        var item = new NotificationBatchWorkItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            0,
            "Title",
            "Body",
            Guid.NewGuid());
        var repository = new StubBatchRepository([item], hasRemaining: true);
        var commandSender = new StubCommandSender();
        var handler = new DispatchNotificationBatchHandler(
            repository,
            new ThrowingSender(),
            commandSender,
            new NotificationMediaReferenceExtractor(),
            new NotificationBatchProcessingOptions());

        await handler.HandleAsync(new DispatchNotificationBatchV1(item.BatchId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.FailedItems, Has.One.EqualTo(item.Id));
            Assert.That(repository.SuccessItems, Is.Empty);
            Assert.That(commandSender.Commands, Has.Count.EqualTo(1));
            Assert.That(commandSender.Commands[0], Is.TypeOf<DispatchNotificationBatchV1>());
        });
    }
}
