// File: backend/Services/Ai/AiService.Api/Mappers/AiRequestMapper.cs
// Mục đích: Chuyển HTTP request completion thành command Application; giá trị thiếu giữ nguyên để Application validate.

using AiService.Api.Contracts.Completions;
using AiService.Application.Services;
using AiService.Application.UseCases.Completions.Create;

namespace AiService.Api.Mappers;

public static class AiRequestMapper
{
    public static CreateAiCompletionCommand ToCommand(CreateAiCompletionRequest request) =>
        new(
            request.Task,
            request.Messages?.Select(ToMessage).ToArray(),
            request.Tools?.Select(tool => new AiChatTool(tool.Name ?? string.Empty, tool.Description ?? string.Empty, tool.ParametersSchema)).ToArray());

    private static AiChatMessage ToMessage(AiCompletionMessageRequest message) =>
        new(
            message.Role ?? string.Empty,
            message.Text,
            message.Parts?.Select(part => new AiChatContentPart(part.Type ?? string.Empty, part.Text, part.MediaType, part.DataBase64, part.FileName)).ToArray(),
            message.ToolCalls?.Select(call => new AiChatToolCall(call.Id ?? string.Empty, call.Name ?? string.Empty, call.ArgumentsJson ?? "{}")).ToArray(),
            message.ToolCallId);
}
