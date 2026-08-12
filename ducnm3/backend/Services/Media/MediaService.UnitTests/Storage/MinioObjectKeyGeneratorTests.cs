using System.Text.RegularExpressions;
using MediaService.Infrastructure.Storage.Minio;

namespace MediaService.UnitTests.Storage;

public partial class MinioObjectKeyGeneratorTests
{
    [Test]
    public void CreateUsesUtcDateUuidAndNormalizedExtension()
    {
        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(2026, 8, 13, 3, 30, 0, TimeSpan.FromHours(7)));
        var generator = new MinioObjectKeyGenerator(timeProvider);

        var objectKey = generator.Create("png");

        Assert.That(
            ObjectKeyPattern().IsMatch(objectKey),
            Is.True,
            $"Unexpected object key: {objectKey}");
    }

    [GeneratedRegex("^2026/08/12/[a-f0-9]{32}\\.png$", RegexOptions.CultureInvariant)]
    private static partial Regex ObjectKeyPattern();

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow.ToUniversalTime();
    }
}
