// File: backend/Services/Ai/AiService.Infrastructure/Persistence/Repositories/EfAiInvocationRepository.cs
// Mục đích: Ghi nhật ký mỗi lần gọi model vào bảng ai_invocations.

using AiService.Application.Repositories;
using AiService.Infrastructure.Persistence.Scaffolded;

namespace AiService.Infrastructure.Persistence.Repositories;

public sealed class EfAiInvocationRepository(AiDbContext db) : IAiInvocationRepository
{
    public async Task AddAsync(AiInvocationRecord record, CancellationToken cancellationToken)
    {
        db.AiInvocations.Add(new AiInvocation
        {
            Id = record.Id,
            TaskName = record.TaskName,
            ModelId = record.ModelId,
            ProviderType = record.ProviderType,
            ModelName = record.ModelName,
            Status = record.Status,
            ErrorCode = record.ErrorCode,
            PromptTokens = record.PromptTokens,
            CompletionTokens = record.CompletionTokens,
            ToolCallCount = record.ToolCallCount,
            LatencyMs = record.LatencyMs,
            CreatedAt = record.CreatedAtUtc,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
