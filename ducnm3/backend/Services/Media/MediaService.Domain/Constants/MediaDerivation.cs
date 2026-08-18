// File: backend/Services/Media/MediaService.Domain/Constants/MediaDerivation.cs
// Mục đích: Khai báo loại tác vụ derivation Media, hiện dùng cho sinh thumbnail sau khi file sẵn sàng.

namespace MediaService.Domain.Constants;

public static class MediaDerivationTypes
{
    public const string Thumbnail = "THUMBNAIL";
}

public static class MediaDerivationStatuses
{
    public const string Queued = "QUEUED";
    public const string Processing = "PROCESSING";
    public const string Ready = "READY";
    public const string Failed = "FAILED";
    public const string NotRequired = "NOT_REQUIRED";
}
