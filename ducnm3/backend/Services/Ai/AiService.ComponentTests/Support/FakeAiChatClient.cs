using AiService.Application.Repositories;
using AiService.Application.Services;

namespace AiService.ComponentTests.Support;

/// <summary>Chat client giả: trả kết quả đã cấu hình (hoặc ném lỗi) và ghi lại request để test kiểm tra.</summary>
internal sealed class FakeAiChatClient : IAiChatClient
{
    public List<AiChatRequest> Requests { get; } = [];

    public AiChatResult Result { get; set; } = new("Xin chào", [], "stop", 11, 7);

    public AiChatException? Failure { get; set; }

    public Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Failure is null ? Task.FromResult(Result) : Task.FromException<AiChatResult>(Failure);
    }
}

internal sealed class InMemoryAiInvocationRepository : IAiInvocationRepository
{
    public List<AiInvocationRecord> Records { get; } = [];

    public bool Throws { get; set; }

    public Task AddAsync(AiInvocationRecord record, CancellationToken cancellationToken)
    {
        if (Throws) throw new InvalidOperationException("database unavailable");
        Records.Add(record);
        return Task.CompletedTask;
    }
}
