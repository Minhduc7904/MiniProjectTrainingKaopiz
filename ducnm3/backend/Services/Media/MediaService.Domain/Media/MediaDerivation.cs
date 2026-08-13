namespace MediaService.Domain.Media;

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
