// File: backend/Services/Ai/AiService.Application/UseCases/Models/AiModelValidation.cs
// Mục đích: Normalize và validate field model dùng chung cho Create và Update.

using AiService.Application.UseCases.Providers;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.UseCases.Models;

internal static class AiModelValidation
{
    public const int ModelNameMaxLength = 100;
    public const int DisplayNameMaxLength = 100;

    public static string NormalizeModelName(string? modelName, List<ApiErrorDetail> details)
    {
        var normalized = modelName?.Trim();
        if (string.IsNullOrEmpty(normalized) || normalized.Length > ModelNameMaxLength || normalized.Any(char.IsWhiteSpace))
        {
            details.Add(new ApiErrorDetail("modelName", $"Model name is required, must not contain whitespace and must be at most {ModelNameMaxLength} characters."));
            return string.Empty;
        }

        return normalized;
    }

    /// <summary>Display name tùy chọn; bỏ trống thì dùng chính model name.</summary>
    public static string NormalizeDisplayName(string? displayName, string modelName, List<ApiErrorDetail> details)
    {
        var normalized = displayName?.Trim();
        if (string.IsNullOrEmpty(normalized)) return modelName;
        if (normalized.Length > DisplayNameMaxLength)
        {
            details.Add(new ApiErrorDetail("displayName", $"Display name must be at most {DisplayNameMaxLength} characters."));
            return string.Empty;
        }

        return normalized;
    }

    public static string NormalizeStatus(string? status, string? defaultStatus, List<ApiErrorDetail> details) =>
        AiProviderValidation.NormalizeStatus(status, defaultStatus, details);
}
