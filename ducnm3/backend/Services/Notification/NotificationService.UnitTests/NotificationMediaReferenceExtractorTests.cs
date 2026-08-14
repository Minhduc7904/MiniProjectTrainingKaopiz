using MediaService.Contracts.Messaging;
using NotificationService.Application;
using NotificationService.Application.Content;

namespace NotificationService.UnitTests;

public sealed class NotificationMediaReferenceExtractorTests
{
    [Test]
    public void ExtractEmbeddedAndAttachedContentUrlsReturnsDistinctReferences()
    {
        var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var sut = new NotificationMediaReferenceExtractor();

        var result = sut.Extract(
            $"![image](/media/api/media/{firstId:D}/content) [file](/media/api/media/{secondId:D}/content) ![again](/media/api/media/{firstId:D}/content)");

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(new NotificationMediaUsageReferenceV1(firstId, NotificationMediaUsageTypes.Embed, 0)));
            Assert.That(result[1], Is.EqualTo(new NotificationMediaUsageReferenceV1(secondId, NotificationMediaUsageTypes.Attachment, 1)));
        });
    }

    [Test]
    public void ExtractExternalMarkdownLinkThrowsValidationError()
    {
        var sut = new NotificationMediaReferenceExtractor();

        var exception = Assert.Throws<NotificationApplicationException>(
            () => sut.Extract("[unsafe](https://example.test/image.png)"));

        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }
}
