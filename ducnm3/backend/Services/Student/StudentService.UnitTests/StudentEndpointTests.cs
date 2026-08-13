using System.Net;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StudentService.Api.Endpoints;
using StudentService.Application.Students;

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
        builder.Services.AddSingleton<GetStudentByIdHandler>();

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

        using var response = await client.GetAsync($"/api/students/{path}");

        Assert.That(response.StatusCode, Is.EqualTo(expectedStatus));
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
        public Task<StudentDetails?> GetByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult<StudentDetails?>(
                studentId == existingStudentId
                    ? new StudentDetails(
                        studentId,
                        "student@example.com",
                        "Student",
                        "ACTIVE")
                    : null);
    }
}
