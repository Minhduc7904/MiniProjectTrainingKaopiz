// File: backend/Services/Notification/NotificationService.Application/Common/Errors/NotificationErrors.cs
// Mục đích: Tập trung tạo lỗi nghiệp vụ Notification với error code và HTTP status thống nhất cho API.

using BuildingBlocks.Contracts.Api;

namespace NotificationService.Application.Common.Errors;

public static class NotificationErrors
{
    public static NotificationApplicationException Validation(string message) =>
        new(ApiErrorCodes.ValidationFailed, message, 400);

    public static NotificationApplicationException BatchNotFound() =>
        new("NOTIFICATION_BATCH_NOT_FOUND", "Notification batch was not found.", 404);

    public static NotificationApplicationException BatchNotTerminal() =>
        new("NOTIFICATION_BATCH_NOT_TERMINAL", "Notification batch is not terminal yet.", 409);

    public static NotificationApplicationException BatchHasNoFailedItems() =>
        new("NOTIFICATION_BATCH_HAS_NO_FAILED_ITEMS", "Notification batch has no failed recipient to retry.", 409);

    public static NotificationApplicationException NotificationNotFound() =>
        new("NOTIFICATION_NOT_FOUND", "Notification was not found.", 404);

    public static NotificationApplicationException StudentServiceUnavailable() =>
        new("STUDENT_SERVICE_UNAVAILABLE", "Student Service is unavailable.", 503);
}
