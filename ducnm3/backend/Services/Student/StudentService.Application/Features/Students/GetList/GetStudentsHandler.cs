namespace StudentService.Application.Features.Students.GetList;

public sealed class GetStudentsHandler(
    IStudentListRepository studentListRepository)
{
    public Task<GetStudentsResult> HandleAsync(
        GetStudentsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return studentListRepository.GetListAsync(query, cancellationToken);
    }
}
