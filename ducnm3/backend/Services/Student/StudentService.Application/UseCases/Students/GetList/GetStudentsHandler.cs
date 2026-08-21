using StudentService.Application.Repositories;

namespace StudentService.Application.UseCases.Students.GetList;

public sealed class GetStudentsHandler(IStudentListRepository repository)
{
    public async Task<GetStudentsResult> HandleAsync(GetStudentsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var page = await repository.GetListAsync(query, cancellationToken);
        return new GetStudentsResult(page.Items, page.TotalItems, page.TotalPages);
    }
}
