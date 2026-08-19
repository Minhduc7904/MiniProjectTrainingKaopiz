// File: backend/Services/Media/MediaService.UnitTests/Clients/StudentLookupClientTests.cs
// Mục đích: Kiểm thử client tra cứu Student Service, gồm mapping response và xử lý lỗi dependency.

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Students;
using BuildingBlocks.Presentation.Api;
using MediaService.Application;
using MediaService.Application.Common.Errors;
using MediaService.Infrastructure.Clients.Student;

namespace MediaService.UnitTests.Clients;

public class StudentLookupClientTests
{
    [Test]
    public async Task GetByIdUsesSharedRouteAndDeserializesSharedContract()
    {
        var studentId = Guid.NewGuid();
        var student = new StudentQueryResponse(
            studentId,
            "student@example.com",
            "Student",
            "ACTIVE");
        var handler = new StubHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(
                    ApiResponseFactory.Success(student, "trace-id")),
            });
        using var httpClient = CreateHttpClient(handler);
        var client = new StudentLookupClient(httpClient);

        var result = await client.GetByIdAsync(
            studentId,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(student));
            Assert.That(
                handler.LastRequestUri?.PathAndQuery,
                Is.EqualTo(ApiRoutes.Students.GetByIdTemplate.Replace(
                    "{studentId}",
                    studentId.ToString("D"),
                    StringComparison.Ordinal)));
        });
    }

    [Test]
    public async Task GetByIdMapsNotFoundToNull()
    {
        var handler = new StubHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.NotFound));
        using var httpClient = CreateHttpClient(handler);
        var client = new StudentLookupClient(httpClient);

        var result = await client.GetByIdAsync(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetByIdMapsDependencyFailureToSafeError()
    {
        var handler = new StubHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var httpClient = CreateHttpClient(handler);
        var client = new StudentLookupClient(httpClient);

        var exception = Assert.ThrowsAsync<MediaApplicationException>(
            () => client.GetByIdAsync(
                Guid.NewGuid(),
                CancellationToken.None));

        Assert.That(
            exception!.ErrorCode,
            Is.EqualTo(MediaErrorCodes.StudentServiceUnavailable));
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler) =>
        new(handler)
        {
            BaseAddress = new Uri("http://student-service/"),
        };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(responseFactory(request));
        }
    }
}
