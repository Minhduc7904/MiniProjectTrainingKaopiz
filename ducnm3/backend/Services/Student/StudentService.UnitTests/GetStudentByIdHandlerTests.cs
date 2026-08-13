using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Students;
using StudentService.Application.Features.Students.GetById;

namespace StudentService.UnitTests;

public class GetStudentByIdHandlerTests
{
    [Test]
    public async Task ExistingStudentIsReturned()
    {
        var student = new StudentQueryResponse(
            Guid.NewGuid(),
            "student@example.com",
            "Student",
            "ACTIVE");
        var handler = new GetStudentByIdHandler(new StubRepository(student));

        var result = await handler.HandleAsync(
            student.Id,
            CancellationToken.None);

        Assert.That(result, Is.EqualTo(student));
    }

    [Test]
    public void MissingStudentReturnsNotFound()
    {
        var handler = new GetStudentByIdHandler(new StubRepository(null));

        var exception = Assert.ThrowsAsync<StudentApplicationException>(
            () => handler.HandleAsync(
                Guid.NewGuid(),
                CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(StudentErrorCodes.NotFound));
    }

    [Test]
    public void EmptyStudentIdReturnsValidationError()
    {
        var handler = new GetStudentByIdHandler(new StubRepository(null));

        var exception = Assert.ThrowsAsync<StudentApplicationException>(
            () => handler.HandleAsync(Guid.Empty, CancellationToken.None));

        Assert.That(
            exception!.ErrorCode,
            Is.EqualTo(ApiErrorCodes.ValidationFailed));
    }

    private sealed class StubRepository(StudentQueryResponse? student)
        : IStudentRepository
    {
        public Task<StudentQueryResponse?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult(student);
    }
}
