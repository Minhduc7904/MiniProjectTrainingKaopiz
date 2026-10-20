// File: backend/Services/Ai/AiService.Application/Common/Errors/AiErrorCodes.cs
// Mục đích: Khai báo error code nghiệp vụ AI ổn định cho API consumer.

namespace AiService.Application.Common.Errors;

public static class AiErrorCodes
{
    public const string ProviderNotFound = "AI_PROVIDER_NOT_FOUND";
    public const string ProviderNameConflict = "AI_PROVIDER_NAME_CONFLICT";
    public const string ProviderInUse = "AI_PROVIDER_IN_USE";
    public const string ModelNotFound = "AI_MODEL_NOT_FOUND";
    public const string ModelConflict = "AI_MODEL_CONFLICT";
    public const string ModelInUse = "AI_MODEL_IN_USE";
    public const string BindingNotFound = "AI_SERVICE_BINDING_NOT_FOUND";
    public const string ReferenceInactive = "AI_REFERENCE_INACTIVE";
}
