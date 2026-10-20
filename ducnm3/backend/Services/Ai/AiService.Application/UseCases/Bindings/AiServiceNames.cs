// File: backend/Services/Ai/AiService.Application/UseCases/Bindings/AiServiceNames.cs
// Mục đích: Normalize service name của binding (trim, lower-case) dùng chung cho mọi use case binding.

namespace AiService.Application.UseCases.Bindings;

internal static class AiServiceNames
{
    public static string Normalize(string? serviceName) =>
        serviceName?.Trim().ToLowerInvariant() ?? string.Empty;
}
