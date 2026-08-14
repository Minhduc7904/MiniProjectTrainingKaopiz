namespace MediaService.Application.Abstractions.Urls;

public sealed record MediaUrl(string Value, DateTime? ExpiresAtUtc);
