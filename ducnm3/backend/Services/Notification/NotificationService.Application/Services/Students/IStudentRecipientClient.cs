// File: backend/Services/Notification/NotificationService.Application/Services/Students/IStudentRecipientClient.cs
// Mục đích: Định nghĩa port đọc recipient pages từ Student Service cho quá trình snapshot Notification Batch.

namespace NotificationService.Application.Services.Students;

public interface IStudentRecipientClient
{
    IAsyncEnumerable<IReadOnlyList<Guid>> GetActiveStudentIdPagesAsync(
        CancellationToken cancellationToken);
}
