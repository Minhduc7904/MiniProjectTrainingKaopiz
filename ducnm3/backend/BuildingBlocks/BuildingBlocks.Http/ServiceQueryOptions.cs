namespace BuildingBlocks.Http;

/// <summary>Options của HTTP query client lấy từ <c>Communication:HttpQuery</c>.</summary>
public sealed class ServiceQueryOptions
{
    public const string SectionName = "Communication:HttpQuery";

    public int TimeoutSeconds { get; init; } = 5;

    public int RetryCount { get; init; } = 3;

    public int RetryDelayMilliseconds { get; init; } = 200;

    /// <summary>Kiểm tra giới hạn timeout và retry để phát hiện cấu hình nguy hiểm ngay lúc khởi động.</summary>
    public void Validate()
    {
        if (TimeoutSeconds is < 1 or > 120)
        {
            throw new InvalidOperationException(
                "Communication:HttpQuery:TimeoutSeconds must be between 1 and 120.");
        }

        if (RetryCount is < 0 or > 10)
        {
            throw new InvalidOperationException(
                "Communication:HttpQuery:RetryCount must be between 0 and 10.");
        }

        if (RetryDelayMilliseconds is < 1 or > 30_000)
        {
            throw new InvalidOperationException(
                "Communication:HttpQuery:RetryDelayMilliseconds must be between 1 and 30000.");
        }
    }
}
