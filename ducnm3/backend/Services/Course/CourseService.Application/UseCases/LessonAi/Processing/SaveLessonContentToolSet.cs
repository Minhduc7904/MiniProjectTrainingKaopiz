// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Processing/SaveLessonContentToolSet.cs
// Mục đích: Tool save_lesson_content: cho model ghi Markdown vào đúng Lesson của job qua UpdateLessonHandler (lessonId do server gắn).

using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Ai;
using CourseService.Application.UseCases.Lessons.Update;

namespace CourseService.Application.UseCases.LessonAi.Processing;

public sealed class SaveLessonContentToolSet(
    LessonAiJobRecord job,
    ILessonCommandRepository lessonRepository,
    ILessonAiRepository aiRepository,
    UpdateLessonHandler updateLessonHandler) : ILessonAiToolSet
{
    public const string ToolName = "save_lesson_content";
    public const int MinLength = 100;
    public const int MaxLength = 500_000;

    private static readonly JsonElement Schema = JsonDocument.Parse("""
        {
          "type": "object",
          "properties": {
            "markdown": { "type": "string", "description": "The complete lesson in Markdown." }
          },
          "required": ["markdown"]
        }
        """).RootElement.Clone();

    public IReadOnlyList<AiToolDefinition> Definitions { get; } =
        [new AiToolDefinition(ToolName, "Save the finished lesson as the content of the current lesson.", Schema)];

    public bool Succeeded { get; private set; }

    public async Task<string> ExecuteAsync(string toolName, string argumentsJson, CancellationToken cancellationToken)
    {
        if (toolName != ToolName) return Error($"Unknown tool '{toolName}'.");
        if (Succeeded) return Error("The lesson was already saved. Do not call this tool again.");

        string? markdown;
        try
        {
            using var document = JsonDocument.Parse(argumentsJson);
            markdown = document.RootElement.TryGetProperty("markdown", out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return Error("Arguments must be a JSON object with a string property 'markdown'.");
        }

        if (string.IsNullOrWhiteSpace(markdown) || markdown.Trim().Length < MinLength)
        {
            return Error($"markdown must contain the complete lesson (at least {MinLength} characters).");
        }

        if (markdown.Length > MaxLength) return Error($"markdown must be at most {MaxLength} characters.");
        if (BrokenEscapeGuard.HasBrokenEscape(markdown)) return Error(BrokenEscapeGuard.Message);

        var current = await lessonRepository.GetAsync(job.CourseId, job.LessonId, cancellationToken);
        if (current is null) return Error("The lesson no longer exists.");

        if (!string.IsNullOrWhiteSpace(current.ContentMarkdown))
        {
            if (!job.OverwriteContent) return Error("The lesson already has content and overwriting was not approved.");
            await aiRepository.SetJobPreviousContentAsync(job.Id, current.ContentMarkdown, cancellationToken);
        }

        try
        {
            await updateLessonHandler.HandleAsync(
                new UpdateLessonCommand(job.CourseId, job.LessonId, false, null, true, markdown, job.CreatedBy),
                cancellationToken);
        }
        catch (CourseApplicationException exception)
        {
            return Error(exception.Message, exception.Details);
        }

        Succeeded = true;
        return JsonSerializer.Serialize(new { ok = true, message = "Lesson content saved." });
    }

    private static string Error(string message, IReadOnlyList<ApiErrorDetail>? details = null) =>
        JsonSerializer.Serialize(new
        {
            ok = false,
            error = message,
            details = details?.Select(detail => new { field = detail.Field, message = detail.Message }),
        });
}
