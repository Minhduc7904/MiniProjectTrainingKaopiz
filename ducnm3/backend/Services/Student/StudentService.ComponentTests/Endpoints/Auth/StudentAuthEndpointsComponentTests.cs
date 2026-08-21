using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StudentService.Api.Contracts.Auth;
using StudentService.Api.Contracts.Auth.GetMe;
using StudentService.Api.Endpoints.Auth.GetMe;
using StudentService.Api.Endpoints.Auth.Login;
using StudentService.Api.Endpoints.Auth.Register;
using StudentService.Application;
using StudentService.Application.Repositories;
using StudentService.Domain.Entities;

namespace StudentService.ComponentTests.Endpoints.Auth;

public sealed class StudentAuthEndpointsComponentTests
{
    private static readonly Guid StudentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private WebApplication app = null!;
    private HttpClient client = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IStudentRepository>(new StubStudentAuthRepository(StudentId));
        builder.Services.AddStudentApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapRegisterStudent();
        app.MapLoginStudent();
        app.MapGetCurrentStudent();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task RegisterCreatesStudentActorAndLocation()
    {
        using var response = await client.PostAsJsonAsync(
            ApiRoutes.StudentAuth.Register,
            new { email = "new@example.com", displayName = "New Student" },
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<StudentActorResponse>>(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(envelope?.Data.Actor, Is.EqualTo(ActorHeaderTypes.Student));
            Assert.That(response.Headers.Location?.ToString(), Is.EqualTo(ApiRoutes.Students.GetByIdPublicPath(StudentId)));
        });
    }

    [Test]
    public async Task LoginWithStudentIdReturnsStudentActor()
    {
        using var response = await client.PostAsJsonAsync(
            ApiRoutes.StudentAuth.Login,
            new { id = StudentId.ToString("D") },
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<StudentActorResponse>>(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(envelope?.Data, Is.EqualTo(new StudentActorResponse(ActorHeaderTypes.Student, StudentId)));
        });
    }

    [Test]
    public async Task MeRequiresStudentActorAndReturnsNoStoreProfile()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.StudentAuth.Me);
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Student);
        request.Headers.Add(ApiHeaderNames.ActorId, StudentId.ToString("D"));
        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<GetCurrentStudentResponse>>(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(envelope?.Data.Id, Is.EqualTo(StudentId));
            Assert.That(envelope?.Data.Email, Is.EqualTo("student@example.com"));
        });
    }

    [Test]
    public async Task MeWithAdminActorReturnsForbidden()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.StudentAuth.Me);
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, StudentId.ToString("D"));
        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class StubStudentAuthRepository(Guid studentId) : IStudentRepository
    {
        private readonly Student student = new(
            studentId,
            "student@example.com",
            "Student",
            "ACTIVE",
            DateTime.UtcNow,
            DateTime.UtcNow);

        public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<Student?>(id == student.Id ? student : null);

        public Task<Student?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
            Task.FromResult<Student?>(null);

        public Task<Student> CreateAsync(string normalizedEmail, string displayName, CancellationToken cancellationToken) =>
            Task.FromResult(student with { Email = normalizedEmail, DisplayName = displayName });
    }
}
