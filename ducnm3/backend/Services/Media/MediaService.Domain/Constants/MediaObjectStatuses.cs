// File: backend/Services/Media/MediaService.Domain/Constants/MediaObjectStatuses.cs
// Mục đích: Khai báo trạng thái vòng đời object Media như pending, ready và failed để điều khiển flow upload.

namespace MediaService.Domain.Constants;

public static class MediaObjectStatuses
{
    public const string Pending = "PENDING";
    public const string Ready = "READY";
    public const string Failed = "FAILED";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal)
        {
            Pending, Ready, Failed,
        };
}
