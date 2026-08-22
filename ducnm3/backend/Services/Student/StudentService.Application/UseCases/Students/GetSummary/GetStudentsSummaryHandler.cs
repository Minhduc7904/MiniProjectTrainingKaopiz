using StudentService.Application.Repositories;

namespace StudentService.Application.UseCases.Students.GetSummary;

public sealed class GetStudentsSummaryHandler(IStudentSummaryRepository repository)
{
    public async Task<StudentsSummaryResult> HandleAsync(CancellationToken cancellationToken) =>
        new(await repository.CountAsync(cancellationToken));
}

public sealed record StudentsSummaryResult(long TotalStudents);
