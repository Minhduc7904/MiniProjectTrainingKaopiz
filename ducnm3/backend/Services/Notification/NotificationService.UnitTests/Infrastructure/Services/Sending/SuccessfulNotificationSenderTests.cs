// File: backend/Services/Notification/NotificationService.UnitTests/Infrastructure/Services/Sending/SuccessfulNotificationSenderTests.cs
// Mục đích: Bảo vệ sender mặc định hoàn tất delivery nội bộ mà không giả lập lỗi theo recipient.

#pragma warning disable CA1707

using NotificationService.Infrastructure.Services.Sending;

namespace NotificationService.UnitTests.Infrastructure.Services.Sending;

public sealed class SuccessfulNotificationSenderTests
{
    [Test]
    public async Task SendAsync_AnyRecipient_CompletesWithoutFailure()
    {
        // Arrange
        var sender = new SuccessfulNotificationSender();
        var studentId = Guid.Parse("a23f9390-b3ae-57a5-a74a-6f3e69226a8e");

        // Act
        await sender.SendAsync(studentId, 1, CancellationToken.None);

        // Assert
        Assert.Pass();
    }
}
