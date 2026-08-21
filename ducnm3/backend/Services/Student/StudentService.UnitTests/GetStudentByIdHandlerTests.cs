using BuildingBlocks.Contracts.Api;
using StudentService.Application.Common.Errors;
using StudentService.Application.Repositories;
using StudentService.Application.UseCases.Students.GetById;
using StudentService.Domain.Entities;

namespace StudentService.UnitTests;

public class GetStudentByIdHandlerTests
{
    [Test]
    public async Task ExistingStudentIsReturned()
    {
        var student = new Student(
            Guid.NewGuid(),
            "student@example.com",
            "Student",
            "ACTIVE",
            DateTime.UtcNow,
            DateTime.UtcNow);
        var handler = new GetStudentByIdHandler(new StubRepository(student));

        var result = await handler.HandleAsync(
            new GetStudentByIdQuery(student.Id),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(student.Id));
            Assert.That(result.Email, Is.EqualTo(student.Email));
            Assert.That(result.Status, Is.EqualTo(student.Status));
        });
    }

    [Test]
    public void MissingStudentReturnsNotFound()
    {
        var handler = new GetStudentByIdHandler(new StubRepository(null));

        var exception = Assert.ThrowsAsync<StudentApplicationException>(
            () => handler.HandleAsync(
                new GetStudentByIdQuery(Guid.NewGuid()),
                CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(StudentErrorCodes.NotFound));
    }

    [Test]
    public void EmptyStudentIdReturnsValidationError()
    {
        var handler = new GetStudentByIdHandler(new StubRepository(null));

        var exception = Assert.ThrowsAsync<StudentApplicationException>(
            () => handler.HandleAsync(new GetStudentByIdQuery(Guid.Empty), CancellationToken.None));

        Assert.That(
            exception!.ErrorCode,
            Is.EqualTo(ApiErrorCodes.ValidationFailed));
    }

    private sealed class StubRepository(Student? student)
        : IStudentRepository
    {
        public Task<Student?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult(student);

        public Task<Student?> GetByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken) =>
            Task.FromResult<Student?>(null);

        public Task<Student> CreateAsync(
            string normalizedEmail,
            string displayName,
            CancellationToken cancellationToken) =>
            Task.FromException<Student>(new NotSupportedException());
    }
}
