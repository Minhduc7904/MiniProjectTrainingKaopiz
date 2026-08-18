// File: backend/Services/Notification/NotificationService.Application/Common/Errors/NotificationApplicationException.cs
// Mục đích: Biểu diễn exception nghiệp vụ Notification để middleware chuyển thành response lỗi chuẩn.

using BuildingBlocks.Contracts.Api;

namespace NotificationService.Application;

public sealed class NotificationApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode,
    IReadOnlyList<ApiErrorDetail>? details = null)
    : ApiException(errorCode, safeMessage, statusCode, details);
