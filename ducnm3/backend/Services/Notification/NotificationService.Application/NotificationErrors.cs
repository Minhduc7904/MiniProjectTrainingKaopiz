using BuildingBlocks.Contracts.Api;

namespace NotificationService.Application;

public static class NotificationErrors
{
    public static NotificationApplicationException Validation(string message) =>
        new(ApiErrorCodes.ValidationFailed, message, 400);

    public static NotificationApplicationException BatchNotFound() =>
        new("NOTIFICATION_BATCH_NOT_FOUND", "Notification batch was not found.", 404);

    public static NotificationApplicationException NotificationNotFound() =>
        new("NOTIFICATION_NOT_FOUND", "Notification was not found.", 404);

    public static NotificationApplicationException StudentServiceUnavailable() =>
        new("STUDENT_SERVICE_UNAVAILABLE", "Student Service is unavailable.", 503);
}
