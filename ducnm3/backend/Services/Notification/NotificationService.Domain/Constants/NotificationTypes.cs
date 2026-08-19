// File: backend/Services/Notification/NotificationService.Domain/Constants/NotificationTypes.cs
// Mục đích: Khai báo các loại notification được hệ thống hỗ trợ để validate request và lưu dữ liệu nhất quán.

namespace NotificationService.Domain.Constants;

public static class NotificationSourceTypes
{
    public const string Bulk = "BULK";
    public const string Direct = "SINGLE";
}

public static class NotificationStatuses
{
    public const string Read = "READ";
    public const string Unread = "UNREAD";
}

public static class NotificationBatchStatuses
{
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
    public const string PartialFailed = "PARTIAL_FAILED";
    public const string Pending = "PENDING";
    public const string Processing = "PROCESSING";
    public const string SnapshotReady = "SNAPSHOT_READY";
    public const string Snapshotting = "SNAPSHOTTING";
}

public static class NotificationBatchItemStatuses
{
    public const string Failed = "FAILED";
    public const string Pending = "PENDING";
    public const string Processing = "PROCESSING";
    public const string Retry = "RETRY";
    public const string Success = "SUCCESS";
}

public static class NotificationBatchStepStatuses
{
    public const string Pending = "PENDING";
    public const string Running = "RUNNING";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
}

public static class NotificationTargetScopes
{
    public const string AllStudents = "ALL_STUDENTS";
    public const string FailedRecipients = "FAILED_RECIPIENTS";
}
