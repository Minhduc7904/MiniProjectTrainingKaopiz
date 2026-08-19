// File: backend/Services/Course/CourseService.Application/Common/Errors/CourseApplicationException.cs
// Mục đích: Biểu diễn lỗi Application Course có HTTP status để middleware tạo API error envelope.

using BuildingBlocks.Contracts.Api;

namespace CourseService.Application.Common.Errors;

public sealed class CourseApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode,
    IReadOnlyList<ApiErrorDetail>? details = null)
    : ApiException(errorCode, safeMessage, statusCode, details);
