using MediaService.Application.Common.Errors;
using MediaService.Application.Services.MediaUsages;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Constants;

namespace MediaService.UnitTests.Application.Services;

public sealed class MarkdownMediaUsageOwnerPolicyTests
{
    [TestCase(MarkdownMediaUsageOwnerTypes.LessonContent)]
    [TestCase(MarkdownMediaUsageOwnerTypes.QuizContent)]
    public void ValidateCourseMarkdownOwnerTypesAreAccepted(string ownerType) =>
        Assert.DoesNotThrow(() => MarkdownMediaUsageOwnerPolicy.Validate(MarkdownMediaUsageOwnerServices.Course, ownerType));

    [Test]
    public void ValidateQuizContentFromAnotherServiceIsRejected() =>
        Assert.Throws<MediaApplicationException>(() =>
            MarkdownMediaUsageOwnerPolicy.Validate(MediaOwnerServices.Notification, MarkdownMediaUsageOwnerTypes.QuizContent));

    [Test]
    public void ValidateUnknownOwnerTypeIsRejected() =>
        Assert.Throws<MediaApplicationException>(() =>
            MarkdownMediaUsageOwnerPolicy.Validate(MarkdownMediaUsageOwnerServices.Course, "QUIZ_ANSWER"));
}
