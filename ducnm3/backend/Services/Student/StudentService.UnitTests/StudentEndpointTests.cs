using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StudentService.Api.Contracts.Students;
using StudentService.Api.Endpoints.Students.GetById;
using StudentService.Application;
using StudentService.Application.Repositories;
using StudentService.Domain.Entities;

namespace StudentService.UnitTests;

public sealed class StudentEndpointTests
{
    private readonly Guid existingStudentId = Guid.NewGuid();
    private WebApplication app = null!;
    private HttpClient client = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IStudentRepository>(
            new EndpointRepository(existingStudentId));
        builder.Services.AddStudentApplication();

        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGetStudentById();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [TestCaseSource(nameof(Cases))]
    public async Task GetStudentMapsExpectedStatus(
        string pathType,
        HttpStatusCode expectedStatus)
    {
        var path = pathType switch
        {
            "existing" => existingStudentId.ToString(),
            "missing" => Guid.NewGuid().ToString(),
            _ => "not-a-uuid",
        };

        var requestPath = pathType == "invalid"
            ? ApiRoutes.Students.GetByIdTemplate.Replace("{studentId}", path)
            : ApiRoutes.Students.GetByIdServicePath(Guid.Parse(path));
        using var response = await client.GetAsync(requestPath);

        Assert.That(response.StatusCode, Is.EqualTo(expectedStatus));
    }

    [Test]
    public async Task ExistingStudentUsesSharedResponseContract()
    {
        using var response = await client.GetAsync(
            ApiRoutes.Students.GetByIdServicePath(existingStudentId));
        var envelope = await response.Content.ReadFromJsonAsync<
            ApiResponse<StudentResponse>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(envelope, Is.Not.Null);
            Assert.That(envelope!.Data.Id, Is.EqualTo(existingStudentId));
            Assert.That(envelope.Data.Email, Is.EqualTo("student@example.com"));
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private static IEnumerable<TestCaseData> Cases()
    {
        yield return new TestCaseData("existing", HttpStatusCode.OK);
        yield return new TestCaseData("missing", HttpStatusCode.NotFound);
        yield return new TestCaseData("invalid", HttpStatusCode.BadRequest);
    }

    private sealed class EndpointRepository(Guid existingStudentId)
        : IStudentRepository
    {
        public Task<Student?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult<Student?>(
                studentId == existingStudentId
                    ? new Student(
                        studentId,
                        "student@example.com",
                        "Student",
                        "ACTIVE",
                        DateTime.UtcNow,
                        DateTime.UtcNow)
                    : null);

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
