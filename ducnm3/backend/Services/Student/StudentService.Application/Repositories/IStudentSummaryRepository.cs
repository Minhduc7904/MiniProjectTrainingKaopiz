namespace StudentService.Application.Repositories;

public interface IStudentSummaryRepository
{
    Task<long> CountAsync(CancellationToken cancellationToken);
}
