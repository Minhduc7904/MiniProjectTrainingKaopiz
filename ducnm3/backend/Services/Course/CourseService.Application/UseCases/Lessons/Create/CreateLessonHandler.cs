using CourseService.Application.Repositories;
using CourseService.Application.Services.Content;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.UseCases.Lessons.Create;

public sealed class CreateLessonHandler(
    ILessonCommandRepository repository,
    ICommandSender commandSender)
{
    internal static string? NormalizeMarkdown(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    public async Task<LessonCreateRecord> HandleAsync(
        Guid courseId,
        string? title,
        string? contentMarkdown,
        uint? displayOrder,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty)
        {
            throw new ArgumentException("courseId must be a valid UUID.", nameof(courseId));
        }

        var normalizedTitle = title?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle) || normalizedTitle.Length > 200)
        {
            throw new ArgumentException("title is required and must be at most 200 characters.", nameof(title));
        }

        var result = await repository.CreateAsync(
            courseId,
            normalizedTitle,
            NormalizeMarkdown(contentMarkdown),
            displayOrder,
            cancellationToken) ?? throw new KeyNotFoundException("Course was not found.");
        var references = LessonMediaReferenceExtractor.Extract(contentMarkdown);
        if (references.Count > 0)
        {
            await commandSender.SendAsync(
                ServiceNames.Media,
                new RegisterCourseLessonMediaUsageV1(result.Id, createdBy, references),
                cancellationToken);
        }

        return result;
    }
}
