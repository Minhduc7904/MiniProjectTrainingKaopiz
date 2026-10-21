// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Documents/SyncLessonAiDocumentsHandler.cs
// Mục đích: Đưa các tệp đính kèm mới của Lesson vào hàng chờ trích xuất (idempotent) và tùy chọn thử lại tài liệu bị lỗi.

using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.LessonAi.Documents;

public sealed record SyncLessonAiDocumentsCommand(Guid CourseId, Guid LessonId, Guid ActorId, bool RetryFailed);

public sealed class SyncLessonAiDocumentsHandler(
    ILessonCommandRepository lessonRepository,
    ICourseMediaReader mediaReader,
    ILessonAiRepository repository)
{
    public async Task<IReadOnlyList<LessonAiDocumentView>> HandleAsync(SyncLessonAiDocumentsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.CourseId == Guid.Empty || command.LessonId == Guid.Empty || command.ActorId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        _ = await lessonRepository.GetAsync(command.CourseId, command.LessonId, cancellationToken) ?? throw CourseErrors.LessonNotFound();
        var attachments = await mediaReader.GetLessonAttachmentsAsync(command.LessonId, cancellationToken);

        var documents = attachments
            .DistinctBy(attachment => attachment.MediaId)
            .Select(attachment =>
            {
                var supported = LessonAiDocumentKinds.Classify(attachment.ContentType, attachment.OriginalFileName) != LessonAiDocumentKind.Unsupported;
                return new NewLessonAiDocument(
                    attachment.MediaId,
                    Truncate(attachment.OriginalFileName, 255),
                    Truncate(attachment.ContentType, 150),
                    supported ? LessonAiDocumentStatuses.Pending : LessonAiDocumentStatuses.Unsupported,
                    supported ? null : LessonAiDocumentKinds.UnsupportedErrorCode,
                    supported ? null : LessonAiDocumentKinds.UnsupportedMessage);
            })
            .ToArray();

        await repository.AddMissingDocumentsAsync(command.LessonId, documents, command.ActorId, cancellationToken);
        if (command.RetryFailed)
        {
            await repository.RetryFailedDocumentsAsync(command.LessonId, attachments.Select(attachment => attachment.MediaId).ToArray(), cancellationToken);
        }

        var records = await repository.GetDocumentsAsync(command.LessonId, cancellationToken);
        return LessonAiDocumentViewBuilder.Build(attachments, records);
    }

    private static string Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) ? "unknown" : value.Length <= maxLength ? value : value[..maxLength];
}
