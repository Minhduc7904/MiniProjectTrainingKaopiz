namespace MediaService.Application.Features.Derivations;

public sealed class MediaThumbnailOptions
{
    public const string SectionName = "MediaThumbnail";

    public int MaxWidth { get; init; } = 640;

    public int MaxHeight { get; init; } = 640;

    public int WebpQuality { get; init; } = 80;

    public double VideoCapturePercent { get; init; } = 0.10;

    public int VideoCaptureMinSeconds { get; init; } = 1;

    public int VideoCaptureMaxSeconds { get; init; } = 30;

    public int ProcessTimeoutSeconds { get; init; } = 120;

    public void Validate()
    {
        if (MaxWidth <= 0 || MaxHeight <= 0)
        {
            throw new InvalidOperationException(
                $"{SectionName} dimensions must be positive.");
        }

        if (WebpQuality is < 1 or > 100)
        {
            throw new InvalidOperationException(
                $"{SectionName}:WebpQuality must be between 1 and 100.");
        }

        if (VideoCapturePercent is <= 0 or > 1 ||
            VideoCaptureMinSeconds < 0 ||
            VideoCaptureMaxSeconds < VideoCaptureMinSeconds ||
            ProcessTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                $"{SectionName} process configuration is invalid.");
        }
    }
}
