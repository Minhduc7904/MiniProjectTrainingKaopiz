using System.Net;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Endpoints.Courses.DeleteCourse;
using CourseService.Application;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using MediaService.Contracts.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CourseService.ComponentTests.Endpoints;

public sealed class DeleteCourseEndpointComponentTests
{
    private static readonly Guid CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LessonId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ActorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid UsageId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private WebApplication app = null!;
    private HttpClient client = null!;
    private StubCourseCommandRepository repository = null!;
    private StubCommandSender commandSender = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        repository = new StubCourseCommandRepository();
        commandSender = new StubCommandSender();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<ICourseCommandRepository>(repository);
        builder.Services.AddSingleton<ICourseMediaReader>(new StubCourseMediaReader());
        builder.Services.AddSingleton<ICommandSender>(commandSender);
        builder.Services.AddCourseApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapDeleteCourse();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task Delete_ExistingCourse_ReturnsAcceptedAndQueuesUsageCleanup()
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, ApiRoutes.Courses.ByIdServicePath(CourseId));
        request.Headers.Add(ApiHeaderNames.ActorId, ActorId.ToString("D"));

        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);

        var command = commandSender.Commands.Single() as DeleteMediaUsagesByIdsV1;
        var body = await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken);
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(body, Is.Empty);
            Assert.That(repository.Deleted, Is.True);
            Assert.That(command?.UsageIds, Is.EquivalentTo([UsageId]));
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class StubCourseCommandRepository : ICourseCommandRepository
    {
        public bool Deleted { get; private set; }

        public Task<CourseCommandRecord?> GetAsync(Guid courseId, CancellationToken cancellationToken) =>
            Task.FromResult<CourseCommandRecord?>(new CourseCommandRecord(courseId, "Backend Fundamentals", null, "DRAFT", DateTime.UtcNow, DateTime.UtcNow));

        public Task<IReadOnlyList<Guid>> GetLessonIdsAsync(Guid courseId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Guid>>([LessonId]);

        public Task<bool> DeleteAsync(Guid courseId, CancellationToken cancellationToken)
        {
            Deleted = true;
            return Task.FromResult(true);
        }

        public Task<CourseCommandRecord> CreateAsync(string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<CourseCommandRecord?> UpdateAsync(Guid courseId, string name, string? descriptionMarkdown, string status, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class StubCourseMediaReader : ICourseMediaReader
    {
        public Task<IReadOnlyList<Guid>> GetActiveUsageIdsAsync(IReadOnlyList<CourseMediaUsageOwnerScope> owners, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Guid>>([UsageId]);

        public Task<CourseMediaSet> GetAsync(Guid courseId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyDictionary<Guid, CourseMediaSet>> GetManyAsync(IReadOnlyList<Guid> courseIds, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<CourseMediaAsset>> GetLessonAttachmentsAsync(Guid lessonId, CancellationToken cancellationToken) => throw new NotSupportedException();
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
