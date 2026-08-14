namespace MediaService.Application.Abstractions.Persistence;

public sealed record MediaUsageUrlRecord(
    MediaUsageRecord Usage,
    MediaRecord Media);
