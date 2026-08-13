namespace StudentService.Application.Features.Students.GetList;

public interface IStudentListRepository
{
    Task<GetStudentsResult> GetListAsync(
        GetStudentsQuery query,
        CancellationToken cancellationToken);
}
