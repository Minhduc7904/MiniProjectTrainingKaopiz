// File: backend/Services/Ai/AiService.Application/Repositories/IAiInvocationRepository.cs
// Mục đích: Port ghi nhật ký mỗi lần gọi model để thống kê token, độ trễ và tỷ lệ lỗi.

namespace AiService.Application.Repositories;

public interface IAiInvocationRepository
{
    Task AddAsync(AiInvocationRecord record, CancellationToken cancellationToken);
}
