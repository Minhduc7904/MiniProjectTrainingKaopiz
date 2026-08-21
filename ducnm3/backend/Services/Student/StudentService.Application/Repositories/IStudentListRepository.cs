using StudentService.Application.UseCases.Students.GetList;

namespace StudentService.Application.Repositories;

public interface IStudentListRepository
{
    Task<StudentListPage> GetListAsync(GetStudentsQuery query, CancellationToken cancellationToken);
}
