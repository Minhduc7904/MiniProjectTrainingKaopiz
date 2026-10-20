// File: backend/Services/Ai/AiService.Application/Common/Errors/AiApplicationException.cs
// Mục đích: Biểu diễn lỗi Application AI có HTTP status để middleware tạo API error envelope.

using BuildingBlocks.Contracts.Api;

namespace AiService.Application.Common.Errors;

public sealed class AiApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode,
    IReadOnlyList<ApiErrorDetail>? details = null)
    : ApiException(errorCode, safeMessage, statusCode, details);
