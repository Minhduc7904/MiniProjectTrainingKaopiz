// File: backend/Services/Ai/AiService.Application/Common/Errors/AiErrors.cs
// Mục đích: Tập trung tạo lỗi AI an toàn, nhất quán với Common Errors của các service khác.

using AiService.Application.Services;
using BuildingBlocks.Contracts.Api;

namespace AiService.Application.Common.Errors;

public static class AiErrors
{
    public static AiApplicationException ValidationFailed(
        IReadOnlyList<ApiErrorDetail> details) =>
        new(
            ApiErrorCodes.ValidationFailed,
            ApiErrorMessages.ValidationFailed,
            400,
            details);

    public static AiApplicationException ProviderNotFound() =>
        new(AiErrorCodes.ProviderNotFound, "AI provider was not found.", 404);

    public static AiApplicationException ProviderNameConflict() =>
        new(AiErrorCodes.ProviderNameConflict, "AI provider name already exists.", 409);

    public static AiApplicationException ProviderInUse() =>
        new(AiErrorCodes.ProviderInUse, "AI provider still has models.", 409);

    public static AiApplicationException ModelNotFound() =>
        new(AiErrorCodes.ModelNotFound, "AI model was not found.", 404);

    public static AiApplicationException ModelConflict() =>
        new(AiErrorCodes.ModelConflict, "AI model already exists for this provider.", 409);

    public static AiApplicationException ModelInUse() =>
        new(AiErrorCodes.ModelInUse, "AI model is still bound to a service.", 409);

    public static AiApplicationException BindingNotFound() =>
        new(AiErrorCodes.BindingNotFound, "AI service binding was not found.", 404);

    public static AiApplicationException ReferenceInactive() =>
        new(AiErrorCodes.ReferenceInactive, "AI model or provider is inactive.", 409);

    public static AiApplicationException TaskNotConfigured(string taskName) =>
        new(AiErrorCodes.TaskNotConfigured, $"No AI model is assigned to '{taskName}'. Assign one on the AI service bindings page.", 409);

    public static AiApplicationException ProviderKeyUnreadable() =>
        new(AiErrorCodes.ProviderKeyUnreadable, "Stored API key cannot be decrypted. Enter the key again.", 409);

    public static AiApplicationException FromChatFailure(AiChatException exception) =>
        exception.Kind switch
        {
            AiChatFailureKind.AuthFailed => new(AiErrorCodes.ProviderAuthFailed, "AI provider rejected the API key.", 502),
            AiChatFailureKind.RateLimited => new(AiErrorCodes.ProviderRateLimited, "AI provider rate limit was reached. Retry later.", 429),
            AiChatFailureKind.Timeout => new(AiErrorCodes.ProviderTimeout, "AI provider did not respond in time.", 504),
            AiChatFailureKind.Unreachable => new(AiErrorCodes.ProviderUnreachable, "Cannot connect to the AI provider.", 502),
            AiChatFailureKind.Rejected => new(AiErrorCodes.ProviderRejectedRequest, exception.Message, 502),
            _ => new(AiErrorCodes.ProviderInvalidResponse, "AI provider returned an unreadable response.", 502),
        };
}
