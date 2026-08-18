// File: backend/Services/Notification/NotificationService.Application/UseCases/Notifications/Create/CreateNotificationHandler.cs
// Mục đích: Validate Notification trực tiếp, lưu bản ghi UNREAD và phát command đăng ký các Media Usage trong Markdown.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Contracts.Messaging;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.Services.Content;

namespace NotificationService.Application.UseCases.Notifications.Create;

public sealed class CreateNotificationHandler(
    INotificationRepository repository,
    NotificationMediaReferenceExtractor mediaReferenceExtractor,
    ICommandSender commandSender,
    TimeProvider timeProvider)
{
    public async Task<NotificationSummary> HandleAsync(
        CreateNotificationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.StudentId == Guid.Empty || command.CreatedBy == Guid.Empty ||
            string.IsNullOrWhiteSpace(command.Title) || command.Title.Length > 200 ||
            string.IsNullOrWhiteSpace(command.BodyMarkdown))
        {
            throw NotificationErrors.Validation(
                "studentId, createdBy, title and bodyMarkdown are required.");
        }

        var references = mediaReferenceExtractor.Extract(command.BodyMarkdown);
        var result = await repository.CreateAsync(
            new CreateNotificationRecord(
                Guid.NewGuid(),
                command.StudentId,
                command.Title.Trim(),
                command.BodyMarkdown,
                command.CreatedBy,
                timeProvider.GetUtcNow().UtcDateTime),
            cancellationToken);
        if (references.Count > 0)
        {
            await commandSender.SendAsync(
                ServiceNames.Media,
                new RegisterNotificationMediaUsageV1(
                    result.Id,
                    result.CreatedBy,
                    references),
                cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return result;
    }
}
