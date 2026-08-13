using StudentService.Application.Students;

namespace StudentService.UnitTests;

public class GetStudentByIdHandlerTests
{
    [Test]
    public async Task ExistingStudentIsReturned()
    {
        var student = new StudentDetails(
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

        Assert.That(exception!.ErrorCode, Is.EqualTo("STUDENT_NOT_FOUND"));
    }

    private sealed class StubRepository(StudentDetails? student)
        : IStudentRepository
    {
        public Task<StudentDetails?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult(student);
    }
}
