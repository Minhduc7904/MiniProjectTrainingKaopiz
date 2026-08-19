// File: backend/Services/Notification/NotificationService.UnitTests/Infrastructure/NotificationInfrastructureDependencyInjectionTests.cs
// Mục đích: Bảo vệ DI Notification dùng sender mặc định thành công thay vì Fake sender có lỗi mô phỏng.

#pragma warning disable CA1707

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Services.Sending;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Services.Sending;

namespace NotificationService.UnitTests.Infrastructure;

public sealed class NotificationInfrastructureDependencyInjectionTests
{
    [Test]
    public void AddNotificationInfrastructure_DefaultSender_ResolvesSuccessfulSender()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServiceEndpoints:student-service"] = "http://student.test/",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddNotificationInfrastructure(
            configuration,
            "Server=localhost;Database=notification;User Id=user;Password=password;");
        using var provider = services.BuildServiceProvider();

        // Act
        var sender = provider.GetRequiredService<INotificationSender>();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(sender, Is.TypeOf<SuccessfulNotificationSender>());
            Assert.That(sender, Is.Not.TypeOf<FakeNotificationSender>());
        });
    }
}
