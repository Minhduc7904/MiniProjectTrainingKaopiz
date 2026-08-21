using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Endpoints.Courses.CreateCourse;
using CourseService.Application;
using CourseService.Application.Repositories;
using CourseService.Domain.Constants;
using MediaService.Contracts.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CourseService.ComponentTests.Endpoints;

public sealed class CreateCourseEndpointComponentTests
{
    private static readonly Guid ActorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MediaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private WebApplication app = null!;
    private HttpClient client = null!;
    private StubCourseCommandRepository repository = null!;
    private StubCommandSender commandSender = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        repository = new StubCourseCommandRepository(CourseId);
        commandSender = new StubCommandSender();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<ICourseCommandRepository>(repository);
        builder.Services.AddSingleton<ICommandSender>(commandSender);
        builder.Services.AddCourseApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapCreateCourse();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task CreateCourseAlwaysCreatesDraftAndSynchronizesDescriptionMedia()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Courses.ListServicePath())
        {
            Content = JsonContent.Create(new
            {
                name = "  Backend Fundamentals  ",
                descriptionMarkdown = $"![cover]({ApiRoutes.Media.ContentPublicPath(MediaId)})",
                status = CourseStatuses.Published,
            }),
        };
        request.Headers.Add(ApiHeaderNames.ActorId, ActorId.ToString());

        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));
        var synchronization = commandSender.Commands.Single() as SynchronizeMarkdownMediaUsageV1;

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Headers.Location?.OriginalString, Is.EqualTo($"/course/api/courses/{CourseId:D}/details"));
            Assert.That(document.RootElement.GetProperty("data").GetProperty("status").GetString(), Is.EqualTo(CourseStatuses.Draft));
            Assert.That(repository.LastName, Is.EqualTo("Backend Fundamentals"));
            Assert.That(repository.LastStatus, Is.EqualTo(CourseStatuses.Draft));
            Assert.That(commandSender.Commands, Has.Count.EqualTo(1));
            Assert.That(synchronization, Is.Not.Null);
            Assert.That(synchronization?.OwnerId, Is.EqualTo(CourseId));
            Assert.That(synchronization?.OwnerType, Is.EqualTo(MarkdownMediaUsageOwnerTypes.CourseDescription));
            Assert.That(synchronization?.CreatedBy, Is.EqualTo(ActorId));
            Assert.That(synchronization?.Added, Is.EqualTo([
                new MarkdownMediaUsageReferenceV1(MediaId, MarkdownMediaUsageTypes.Embed, 0),
            ]));
            Assert.That(synchronization?.Removed, Is.Empty);
        });
    }

    [Test]
    public async Task CreateCourseWithoutActorHeaderReturnsValidationErrorBeforePersisting()
    {
        using var response = await client.PostAsJsonAsync(
            ApiRoutes.Courses.ListServicePath(),
            new { name = "Backend Fundamentals" },
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.ValidationFailed));
            Assert.That(repository.CreateCallCount, Is.Zero);
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class StubCourseCommandRepository(Guid courseId) : ICourseCommandRepository
    {
        public int CreateCallCount { get; private set; }
        public string? LastName { get; private set; }
        public string? LastStatus { get; private set; }

        public Task<CourseCommandRecord?> GetAsync(Guid courseId, CancellationToken cancellationToken) =>
            Task.FromResult<CourseCommandRecord?>(null);

        public Task<CourseCommandRecord> CreateAsync(string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken)
        {
            CreateCallCount++;
            LastName = name;
            LastStatus = status;
            var now = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc);
            return Task.FromResult(new CourseCommandRecord(courseId, name, descriptionMarkdown, status, now, now));
        }

        public Task<CourseCommandRecord?> UpdateAsync(Guid courseId, string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken) =>
            Task.FromResult<CourseCommandRecord?>(null);

        public Task<IReadOnlyList<Guid>> GetLessonIdsAsync(Guid courseId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Guid>>([]);

        public Task<bool> DeleteAsync(Guid courseId, CancellationToken cancellationToken) =>
            Task.FromResult(false);
    }

    private sealed class StubCommandSender : ICommandSender
    {
        public List<ICommand> Commands { get; } = [];

        public Task SendAsync<TCommand>(string destinationService, TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, ICommand
        {
            Commands.Add(command);
            return Task.CompletedTask;
        }
    }
}
