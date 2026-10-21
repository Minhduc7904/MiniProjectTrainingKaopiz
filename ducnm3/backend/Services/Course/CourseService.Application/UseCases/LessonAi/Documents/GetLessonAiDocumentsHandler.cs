// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Documents/GetLessonAiDocumentsHandler.cs
// Mục đích: Trả danh sách tệp đính kèm của Lesson kèm trạng thái trích xuất văn bản; không ghi dữ liệu.

using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;

namespace CourseService.Application.UseCases.LessonAi.Documents;

public sealed class GetLessonAiDocumentsHandler(
    ILessonCommandRepository lessonRepository,
    ICourseMediaReader mediaReader,
    ILessonAiRepository repository)
{
    public async Task<IReadOnlyList<LessonAiDocumentView>> HandleAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        _ = await lessonRepository.GetAsync(courseId, lessonId, cancellationToken) ?? throw CourseErrors.LessonNotFound();

        var attachments = await mediaReader.GetLessonAttachmentsAsync(lessonId, cancellationToken);
        var records = await repository.GetDocumentsAsync(lessonId, cancellationToken);
        return LessonAiDocumentViewBuilder.Build(attachments, records);
    }
}
