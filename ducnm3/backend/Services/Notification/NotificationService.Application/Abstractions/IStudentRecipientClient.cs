namespace NotificationService.Application.Abstractions;

public interface IStudentRecipientClient
{
    Task<IReadOnlyList<Guid>> GetAllActiveStudentIdsAsync(CancellationToken cancellationToken);
}
