// File: backend/Services/Notification/NotificationService.UnitTests/Application/UseCases/NotificationBatches/Create/CreateNotificationBatchHandlerTests.cs
// Mục đích: Kiểm thử validation, giá trị mặc định và command snapshot của use case tạo Notification Batch.

#pragma warning disable CA1707

using NotificationService.Application.Common.Errors;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Services.Content;
using NotificationService.Application.UseCases.NotificationBatches.Create;
using NotificationService.UnitTests.Application.UseCases.NotificationBatches.TestDoubles;

namespace NotificationService.UnitTests.Application.UseCases.NotificationBatches.Create;

public sealed class CreateNotificationBatchHandlerTests
{
    [Test]
    public void HandleAsync_RejectsScopeOtherThanAllStudents()
    {
        var handler = new CreateNotificationBatchHandler(
            new StubBatchRepository(),
            new StubCommandSender(),
            new NotificationMediaReferenceExtractor(),
            TimeProvider.System);

        var exception = Assert.ThrowsAsync<NotificationApplicationException>(
            () => handler.HandleAsync(
                new CreateNotificationBatchCommand(
                    "Title",
                    "Body",
                    "COURSE_ENROLLED",
                    Guid.NewGuid(),
                    null,
                    null),
                CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task HandleAsync_CreatesPendingBatchAndQueuesSnapshotCommand()
    {
        var repository = new StubBatchRepository();
        var commandSender = new StubCommandSender();
        var handler = new CreateNotificationBatchHandler(
            repository,
            commandSender,
            new NotificationMediaReferenceExtractor(),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreateNotificationBatchCommand(
                "Title",
                "Body",
                "ALL_STUDENTS",
                Guid.NewGuid(),
                null,
                null),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(repository.CreatedRecord, Is.Not.Null);
            Assert.That(repository.CreatedRecord!.BatchSize, Is.EqualTo(500));
            Assert.That(commandSender.Commands, Has.Count.EqualTo(1));
            Assert.That(commandSender.Commands[0], Is.TypeOf<SnapshotNotificationBatchV1>());
            Assert.That(((SnapshotNotificationBatchV1)commandSender.Commands[0]).BatchId, Is.EqualTo(result.Id));
        });
    }
}
