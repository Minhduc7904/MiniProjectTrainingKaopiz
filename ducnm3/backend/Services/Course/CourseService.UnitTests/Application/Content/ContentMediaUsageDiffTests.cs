using BuildingBlocks.Contracts.Api;
using CourseService.Application.Services.Content;
using MediaService.Contracts.Messaging;

namespace CourseService.UnitTests.Application.Content;

public sealed class ContentMediaUsageDiffTests
{
    private static readonly Guid OldMediaId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid NewMediaId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Test]
    public void CreateOldAndNewMarkdownReturnsOnlyAddedAndRemovedReferences()
    {
        // Arrange
        var before = $"[old]({ApiRoutes.Media.ContentPublicPath(OldMediaId)})";
        var after = $"![new]({ApiRoutes.Media.ContentPublicPath(NewMediaId)})";

        // Act
        var result = ContentMediaUsageDiff.Create(before, after);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Added, Is.EqualTo([
                new MarkdownMediaUsageReferenceV1(
                    NewMediaId,
                    MarkdownMediaUsageTypes.Embed,
                    0),
            ]));
            Assert.That(result.Removed, Is.EqualTo([
                new MarkdownMediaUsageReferenceV1(
                    OldMediaId,
                    MarkdownMediaUsageTypes.Attachment,
                    0),
            ]));
        });
    }

    [Test]
    public void CreateUnchangedMarkdownReturnsEmptyDiff()
    {
        // Arrange
        var markdown = $"[media]({ApiRoutes.Media.ContentPublicPath(OldMediaId)})";

        // Act
        var result = ContentMediaUsageDiff.Create(markdown, markdown);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Added, Is.Empty);
            Assert.That(result.Removed, Is.Empty);
        });
    }
}
