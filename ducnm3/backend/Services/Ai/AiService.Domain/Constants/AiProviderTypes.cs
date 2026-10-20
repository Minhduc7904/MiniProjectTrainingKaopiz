// File: backend/Services/Ai/AiService.Domain/Constants/AiProviderTypes.cs
// Mục đích: Khai báo loại AI provider và base URL OpenAI-compatible mặc định của từng loại.

namespace AiService.Domain.Constants;

public static class AiProviderTypes
{
    public const string OpenAi = "OPENAI";
    public const string Gemini = "GEMINI";
    public const string Anthropic = "ANTHROPIC";
    public const string Mistral = "MISTRAL";
    public const string Custom = "CUSTOM";

    public static IReadOnlyList<string> All { get; } = [OpenAi, Gemini, Anthropic, Mistral, Custom];

    public static bool IsSupported(string value) =>
        value is OpenAi or Gemini or Anthropic or Mistral or Custom;

    /// <summary>Trả base URL OpenAI-compatible mặc định; CUSTOM không có mặc định nên trả null.</summary>
    public static string? DefaultBaseUrl(string providerType) =>
        providerType switch
        {
            OpenAi => "https://api.openai.com/v1",
            Gemini => "https://generativelanguage.googleapis.com/v1beta/openai/",
            Anthropic => "https://api.anthropic.com/v1/",
            Mistral => "https://api.mistral.ai/v1",
            _ => null,
        };
}
