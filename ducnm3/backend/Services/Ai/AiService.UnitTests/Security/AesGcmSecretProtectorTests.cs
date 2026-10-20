using System.Security.Cryptography;
using AiService.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace AiService.UnitTests.Security;

public sealed class AesGcmSecretProtectorTests
{
    private static string NewKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(AesGcmSecretProtector.KeySizeInBytes));

    private static AesGcmSecretProtector CreateProtector(string? key = null) =>
        new(Options.Create(new AiEncryptionOptions { Key = key ?? NewKey() }));

    [Test]
    public void ProtectAndUnprotectRoundTripIncludingNonAsciiText()
    {
        var protector = CreateProtector();

        var protectedText = protector.Protect("sk-proj-Aa0_-ñ✓");

        Assert.Multiple(() =>
        {
            Assert.That(protectedText, Does.Not.Contain("sk-proj"));
            Assert.That(protector.Unprotect(protectedText), Is.EqualTo("sk-proj-Aa0_-ñ✓"));
        });
    }

    [Test]
    public void ProtectUsesFreshNoncePerCall()
    {
        var protector = CreateProtector();
        var first = protector.Protect("same-secret");
        var second = protector.Protect("same-secret");

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void UnprotectFailsWhenCiphertextWasTamperedWith()
    {
        var protector = CreateProtector();
        var payload = Convert.FromBase64String(protector.Protect("sk-secret-value"));
        payload[^1] ^= 0x01;

        Assert.Throws<AuthenticationTagMismatchException>(() => protector.Unprotect(Convert.ToBase64String(payload)));
    }

    [Test]
    public void UnprotectFailsWithADifferentMasterKeyOrMalformedPayload()
    {
        var protectedText = CreateProtector().Protect("sk-secret-value");

        Assert.Multiple(() =>
        {
            Assert.Throws<AuthenticationTagMismatchException>(() => CreateProtector().Unprotect(protectedText));
            Assert.Throws<CryptographicException>(() => CreateProtector().Unprotect(Convert.ToBase64String([1, 2, 3])));
        });
    }

    [TestCase("sk-secret-value-1234", "****1234")]
    [TestCase("12345678", "****5678")]
    [TestCase("short", "****")]
    public void MaskExposesOnlyTheLastFourCharactersOfLongKeys(string key, string expected)
    {
        Assert.That(CreateProtector().Mask(key), Is.EqualTo(expected));
    }

    [Test]
    public void ValidatorRejectsMissingOrWrongSizedKeys()
    {
        var validator = new AiEncryptionOptionsValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(null, new AiEncryptionOptions { Key = NewKey() }).Succeeded, Is.True);
            Assert.That(validator.Validate(null, new AiEncryptionOptions { Key = "" }).Failed, Is.True);
            Assert.That(validator.Validate(null, new AiEncryptionOptions { Key = "not-base64!" }).Failed, Is.True);
            Assert.That(validator.Validate(null, new AiEncryptionOptions { Key = Convert.ToBase64String(new byte[16]) }).Failed, Is.True);
        });
    }
}
