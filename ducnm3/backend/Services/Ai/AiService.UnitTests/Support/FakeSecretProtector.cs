using AiService.Application.Services;

namespace AiService.UnitTests.Support;

internal sealed class FakeSecretProtector : ISecretProtector
{
    public const string Prefix = "enc:";

    public string Protect(string plainText) => Prefix + plainText;

    public string Unprotect(string protectedText) => protectedText[Prefix.Length..];

    public string Mask(string plainText) => "****" + plainText[^4..];
}
