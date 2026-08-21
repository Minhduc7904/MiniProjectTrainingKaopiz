using StudentService.Application.Common.Errors;
using StudentService.Application.Repositories;
using StudentService.Application.UseCases.Auth.GetMe;
using StudentService.Application.UseCases.Auth.Login;
using StudentService.Application.UseCases.Auth.Register;
using StudentService.Domain.Entities;

namespace StudentService.UnitTests.Auth;

public sealed class AuthHandlersTests
{
    private static readonly Guid StudentId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public async Task RegisterAsyncValidStudentCreatesActiveStudentActor()
    {
        var repository = new FakeStudentAuthRepository();
        var handler = new RegisterStudentHandler(repository);

        var result = await handler.HandleAsync(
            new RegisterStudentCommand("student@example.com", "Student One"),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.Actor, Is.EqualTo("STUDENT"));
            Assert.That(result.Id, Is.EqualTo(StudentId));
            Assert.That(repository.Created?.Status, Is.EqualTo("ACTIVE"));
        });
    }

    [Test]
    public void LoginAsyncInactiveStudentThrowsNotActive()
    {
        var handler = new LoginStudentHandler(
            new FakeStudentAuthRepository(
                new Student(StudentId, "student@example.com", "Student One", "BLOCKED", DateTime.UtcNow, DateTime.UtcNow)));

        var exception = Assert.ThrowsAsync<StudentApplicationException>(() =>
            handler.HandleAsync(new LoginStudentCommand(StudentId), TestContext.CurrentContext.CancellationToken));

        Assert.That(exception!.ErrorCode, Is.EqualTo(StudentErrorCodes.NotActive));
    }

    [Test]
    public async Task GetMeAsyncActiveStudentReturnsStudentProfile()
    {
        var profile = new Student(
            StudentId,
            "student@example.com",
            "Student One",
            "ACTIVE",
            DateTime.UtcNow,
            DateTime.UtcNow);
        var handler = new GetCurrentStudentHandler(new FakeStudentAuthRepository(profile));

        var result = await handler.HandleAsync(
            new GetCurrentStudentQuery(StudentId),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(StudentId));
            Assert.That(result.Email, Is.EqualTo("student@example.com"));
            Assert.That(result.Status, Is.EqualTo("ACTIVE"));
        });
    }

    private sealed class FakeStudentAuthRepository(Student? profile = null)
        : IStudentRepository
    {
        private readonly Student? profile = profile;

        public Student? Created { get; private set; }

        public Task<Student?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult(profile);

        public Task<Student?> GetByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken) =>
            Task.FromResult<Student?>(null);

        public Task<Student> CreateAsync(
            string normalizedEmail,
            string displayName,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            Created = new Student(StudentId, normalizedEmail, displayName, "ACTIVE", now, now);
            return Task.FromResult(Created);
        }
    }
}
