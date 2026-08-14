namespace NotificationService.Application.Abstractions;

public interface IStudentRecipientClient
{
    IAsyncEnumerable<IReadOnlyList<Guid>> GetActiveStudentIdPagesAsync(
        CancellationToken cancellationToken);
}
