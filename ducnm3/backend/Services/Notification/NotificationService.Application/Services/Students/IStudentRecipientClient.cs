// File: backend/Services/Notification/NotificationService.Application/Services/Students/IStudentRecipientClient.cs
// Mục đích: Triển khai client tích hợp hệ thống ngoài cho IStudentRecipientClient.

namespace NotificationService.Application.Abstractions;

public interface IStudentRecipientClient
{
    IAsyncEnumerable<IReadOnlyList<Guid>> GetActiveStudentIdPagesAsync(
        CancellationToken cancellationToken);
}
