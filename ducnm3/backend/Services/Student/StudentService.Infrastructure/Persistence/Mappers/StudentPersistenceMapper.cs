using StudentService.Domain.Entities;
using DatabaseStudent = StudentService.Infrastructure.Persistence.Scaffolded.Student;

namespace StudentService.Infrastructure.Persistence.Mappers;

public static class StudentPersistenceMapper
{
    public static Student ToDomain(DatabaseStudent source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new Student(source.Id, source.Email, source.DisplayName, source.Status, source.CreatedAt, source.UpdatedAt);
    }
}
