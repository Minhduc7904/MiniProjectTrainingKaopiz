// File: backend/Services/Ai/AiService.Application/Common/PagedResult.cs
// Mục đích: Trả danh sách và metadata offset pagination nội bộ cho use case.

namespace AiService.Application.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    long TotalItems,
    int TotalPages);
