// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Processing/CreateQuizToolSet.cs
// Mục đích: Tool create_quiz: cho model tạo Quiz DRAFT trong đúng Lesson của job qua CreateQuizHandler; lỗi validation trả lại để model tự sửa.

using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Ai;
using CourseService.Application.UseCases.Quizzes;
using CourseService.Application.UseCases.Quizzes.Create;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.LessonAi.Processing;

public sealed class CreateQuizToolSet(LessonAiJobRecord job, CreateQuizHandler createQuizHandler) : ILessonAiToolSet
{
    public const string ToolName = "create_quiz";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly JsonElement Schema = JsonDocument.Parse("""
        {
          "type": "object",
          "properties": {
            "title": { "type": "string", "description": "Quiz title, 2-200 characters." },
            "descriptionMarkdown": { "type": "string", "description": "Optional short intro in Markdown." },
            "questions": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "questionMarkdown": { "type": "string" },
                  "questionType": { "type": "string", "enum": ["SINGLE_CHOICE", "MULTIPLE_CHOICE"] },
                  "points": { "type": "number", "description": "Use 1." },
                  "options": {
                    "type": "array",
                    "items": {
                      "type": "object",
                      "properties": {
                        "optionMarkdown": { "type": "string" },
                        "isCorrect": { "type": "boolean" }
                      },
                      "required": ["optionMarkdown", "isCorrect"]
                    }
                  }
                },
                "required": ["questionMarkdown", "questionType", "points", "options"]
              }
            }
          },
          "required": ["title", "questions"]
        }
        """).RootElement.Clone();

    public IReadOnlyList<AiToolDefinition> Definitions { get; } =
        [new AiToolDefinition(ToolName, "Create the quiz (saved as DRAFT) in the current lesson.", Schema)];

    public bool Succeeded { get; private set; }

    public Guid? QuizId { get; private set; }

    public async Task<string> ExecuteAsync(string toolName, string argumentsJson, CancellationToken cancellationToken)
    {
        if (toolName != ToolName) return Error($"Unknown tool '{toolName}'.");
        if (Succeeded) return Error("The quiz was already created. Do not call this tool again.");

        QuizArguments? arguments;
        try
        {
            arguments = JsonSerializer.Deserialize<QuizArguments>(argumentsJson, JsonOptions);
        }
        catch (JsonException)
        {
            return Error("Arguments do not match the create_quiz schema.");
        }

        if (arguments is null) return Error("Arguments are required.");

        if (HasBrokenEscape(arguments)) return Error(BrokenEscapeGuard.Message);

        var expected = job.QuestionCount ?? 10;
        var count = arguments.Questions?.Count ?? 0;
        if (count != expected) return Error($"The quiz must contain exactly {expected} questions but {count} were provided.");

        var input = new QuizInput(
            arguments.Title,
            arguments.DescriptionMarkdown,
            QuizStatuses.Draft,
            arguments.Questions!
                .Select(question => new QuizQuestionInput(
                    question.QuestionMarkdown,
                    question.QuestionType,
                    question.Points,
                    question.Options?.Select(option => new QuizOptionInput(option.OptionMarkdown, option.IsCorrect)).ToArray()))
                .ToArray());

        try
        {
            var quiz = await createQuizHandler.HandleAsync(
                new CreateQuizCommand(job.CourseId, job.LessonId, job.CreatedBy, input),
                cancellationToken);
            QuizId = quiz.Id;
            Succeeded = true;
            return JsonSerializer.Serialize(new { ok = true, message = "Quiz created as DRAFT." });
        }
        catch (CourseApplicationException exception)
        {
            return Error(exception.Message, exception.Details);
        }
    }

    private static bool HasBrokenEscape(QuizArguments arguments) =>
        BrokenEscapeGuard.HasBrokenEscape(arguments.Title)
        || BrokenEscapeGuard.HasBrokenEscape(arguments.DescriptionMarkdown)
        || (arguments.Questions ?? []).Any(question =>
            BrokenEscapeGuard.HasBrokenEscape(question.QuestionMarkdown)
            || (question.Options ?? []).Any(option => BrokenEscapeGuard.HasBrokenEscape(option.OptionMarkdown)));

    private static string Error(string message, IReadOnlyList<ApiErrorDetail>? details = null) =>
        JsonSerializer.Serialize(new
        {
            ok = false,
            error = message,
            details = details?.Select(detail => new { field = detail.Field, message = detail.Message }),
        });

    private sealed record QuizArguments(string? Title, string? DescriptionMarkdown, IReadOnlyList<QuestionArguments>? Questions);

    private sealed record QuestionArguments(string? QuestionMarkdown, string? QuestionType, decimal? Points, IReadOnlyList<OptionArguments>? Options);

    private sealed record OptionArguments(string? OptionMarkdown, bool? IsCorrect);
}
