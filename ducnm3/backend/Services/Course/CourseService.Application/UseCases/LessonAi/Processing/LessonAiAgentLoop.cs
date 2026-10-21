// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Processing/LessonAiAgentLoop.cs
// Mục đích: Vòng lặp agent: gửi messages + tools cho model, thực thi tool_calls bằng handler của hệ thống, trả kết quả lại cho model đến khi xong.

using CourseService.Application.Services.Ai;

namespace CourseService.Application.UseCases.LessonAi.Processing;

/// <summary>Bộ tool một job được dùng; mỗi tool thực thi qua handler Application có sẵn, không đụng database trực tiếp.</summary>
public interface ILessonAiToolSet
{
    IReadOnlyList<AiToolDefinition> Definitions { get; }

    /// <summary>True khi tool ghi kết quả đã chạy thành công, tức job đạt mục tiêu.</summary>
    bool Succeeded { get; }

    /// <summary>Chạy tool và trả JSON kết quả đưa lại cho model (kể cả lỗi validation để model tự sửa).</summary>
    Task<string> ExecuteAsync(string toolName, string argumentsJson, CancellationToken cancellationToken);
}

public sealed record LessonAiAgentResult(bool Succeeded, int Steps, string? FailureCode, string? FailureMessage);

public sealed class LessonAiAgentLoop(IAiCompletionClient aiClient)
{
    public const int MaxSteps = 6;
    public const string NoResultCode = "AI_NO_RESULT";
    public const string TooManyStepsCode = "AI_TOO_MANY_STEPS";

    /// <param name="onStep">Gọi sau mỗi lượt gọi model với số lượt đã dùng, để ghi tiến độ và gia hạn lease.</param>
    public async Task<LessonAiAgentResult> RunAsync(
        string task,
        string systemPrompt,
        string userPrompt,
        ILessonAiToolSet tools,
        Guid actorId,
        Func<int, CancellationToken, Task> onStep,
        CancellationToken cancellationToken)
    {
        var messages = new List<AiMessage>
        {
            new("system", systemPrompt),
            new("user", userPrompt),
        };

        for (var step = 1; step <= MaxSteps; step++)
        {
            var completion = await aiClient.CompleteAsync(task, messages, tools.Definitions, actorId, cancellationToken);
            await onStep(step, cancellationToken);

            if (completion.ToolCalls.Count == 0)
            {
                // Model trả lời thường: đạt mục tiêu nếu tool ghi đã thành công ở lượt trước, ngược lại là thất bại.
                return tools.Succeeded
                    ? new LessonAiAgentResult(true, step, null, null)
                    : new LessonAiAgentResult(false, step, NoResultCode, "The model finished without saving any result.");
            }

            messages.Add(new AiMessage("assistant", completion.Text, null, completion.ToolCalls));
            foreach (var call in completion.ToolCalls)
            {
                var result = await tools.ExecuteAsync(call.Name, call.ArgumentsJson, cancellationToken);
                messages.Add(new AiMessage("tool", result, null, null, call.Id));
            }

            if (tools.Succeeded)
            {
                // Đã ghi xong; không cần thêm lượt gọi model chỉ để nó nói "xong".
                return new LessonAiAgentResult(true, step, null, null);
            }
        }

        return new LessonAiAgentResult(false, MaxSteps, TooManyStepsCode, "The model did not produce a valid result within the allowed number of steps.");
    }
}
