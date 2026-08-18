// File: backend/Services/Notification/NotificationService.UnitTests/Infrastructure/Services/Sending/FakeNotificationSenderTests.cs
// Mục đích: Khóa quy tắc giả lập lỗi theo hash và số lần thử của FakeNotificationSender dùng trong môi trường phát triển.

#pragma warning disable CA1707

using NotificationService.Infrastructure.Services.Sending;

namespace NotificationService.UnitTests.Infrastructure.Services.Sending;

public sealed class FakeNotificationSenderTests
{
    [Test]
    public void SendAsync_FailsFirstAttemptForStudentHashDivisibleByTwenty()
    {
        var sender = new FakeNotificationSender();
        var studentId = FindStudentId(value => value % 20 == 0);

        Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.SendAsync(studentId, 1, CancellationToken.None));
    }

    [Test]
    public void SendAsync_FailsSecondAttemptForStudentHashDivisibleByOneHundred()
    {
        var sender = new FakeNotificationSender();
        var studentId = FindStudentId(value => value % 100 == 0);

        Assert.ThrowsAsync<InvalidOperationException>(
            () => sender.SendAsync(studentId, 2, CancellationToken.None));
    }

    [Test]
    public async Task SendAsync_SucceedsWhenFailureRuleDoesNotMatch()
    {
        var sender = new FakeNotificationSender();
        var studentId = FindStudentId(value => value % 20 != 0);

        await sender.SendAsync(studentId, 1, CancellationToken.None);
    }

    private static Guid FindStudentId(Func<uint, bool> predicate)
    {
        for (var value = 1; value < 100_000; value++)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            var studentId = new Guid(bytes);
            if (predicate(unchecked((uint)studentId.GetHashCode())))
            {
                return studentId;
            }
        }

        throw new InvalidOperationException("Could not find a deterministic fake-sender test value.");
    }
}
