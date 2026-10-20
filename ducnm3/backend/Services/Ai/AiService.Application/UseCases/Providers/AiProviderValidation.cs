// File: backend/Services/Ai/AiService.Application/UseCases/Providers/AiProviderValidation.cs
// Mục đích: Normalize và validate field provider dùng chung cho Create và Update.

using AiService.Domain.Constants;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Providers;

internal static class AiProviderValidation
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;
    public const int BaseUrlMaxLength = 500;
    public const int ApiKeyMaxLength = 500;

    public static string NormalizeName(string? name, List<ApiErrorDetail> details)
    {
        var normalized = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length < NameMinLength || normalized.Length > NameMaxLength)
        {
            details.Add(new ApiErrorDetail("name", $"Name must be between {NameMinLength} and {NameMaxLength} characters."));
            return string.Empty;
        }

        return normalized;
    }

    public static string NormalizeProviderType(string? providerType, List<ApiErrorDetail> details)
    {
        var normalized = providerType?.Trim().ToUpperInvariant();
        if (normalized is not null && AiProviderTypes.IsSupported(normalized)) return normalized;
        details.Add(new ApiErrorDetail("providerType", $"Provider type must be one of {string.Join(", ", AiProviderTypes.All)}."));
        return string.Empty;
    }

    /// <summary>Dùng base URL người dùng nhập, nếu bỏ trống thì lấy mặc định theo loại; CUSTOM bắt buộc phải nhập.</summary>
    public static string ResolveBaseUrl(string? baseUrl, string providerType, List<ApiErrorDetail> details)
    {
        var normalized = baseUrl?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            var defaultUrl = AiProviderTypes.DefaultBaseUrl(providerType);
            if (defaultUrl is not null) return defaultUrl;
            if (providerType.Length > 0) details.Add(new ApiErrorDetail("baseUrl", "Base URL is required for this provider type."));
            return string.Empty;
        }

        if (normalized.Length > BaseUrlMaxLength
            || !Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https")
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            details.Add(new ApiErrorDetail("baseUrl", "Base URL must be an absolute http(s) URL without credentials."));
            return string.Empty;
        }

        return normalized;
    }

    /// <summary>Trả API key đã trim; khi <paramref name="required"/> là false thì key rỗng nghĩa là giữ key cũ và trả null.</summary>
    public static string? NormalizeApiKey(string? apiKey, bool required, List<ApiErrorDetail> details)
    {
        var normalized = apiKey?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            if (required) details.Add(new ApiErrorDetail("apiKey", "API key is required."));
            return null;
        }

        if (normalized.Length > ApiKeyMaxLength || normalized.Any(char.IsWhiteSpace))
        {
            details.Add(new ApiErrorDetail("apiKey", $"API key must not contain whitespace and must be at most {ApiKeyMaxLength} characters."));
            return null;
        }

        return normalized;
    }

    public static string NormalizeStatus(string? status, string? defaultStatus, List<ApiErrorDetail> details)
    {
        if (string.IsNullOrWhiteSpace(status) && defaultStatus is not null) return defaultStatus;
        var normalized = status?.Trim().ToUpperInvariant();
        if (normalized is not null && AiStatuses.IsSupported(normalized)) return normalized;
        details.Add(new ApiErrorDetail("status", $"Status must be {AiStatuses.Active} or {AiStatuses.Inactive}."));
        return string.Empty;
    }
}
