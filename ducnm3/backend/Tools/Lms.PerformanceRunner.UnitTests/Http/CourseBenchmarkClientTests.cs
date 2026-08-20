using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Lms.PerformanceRunner.Http;

namespace Lms.PerformanceRunner.UnitTests.Http;

public sealed class CourseBenchmarkClientTests
{
    [Test]
    public async Task DownloadReadsIncrementallyAndReportsCsvEvidence()
    {
        const string csv = "\uFEFFid,name,status,createdAtUtc\r\n1,First,PUBLISHED,2026-08-19T00:00:00.0000000Z\r\n2,Second,PUBLISHED,2026-08-19T00:00:00.0000000Z\r\n";
        using var client = new HttpClient(new StubHandler(csv))
        {
            BaseAddress = new Uri("http://localhost:5100"),
        };
        var benchmarkClient = new CourseBenchmarkClient(client);
        var progress = new List<CsvDownloadProgress>();

        var result = await benchmarkClient.DownloadAsync(
            "streaming",
            "PUBLISHED",
            CancellationToken.None,
            new Progress<CsvDownloadProgress>(progress.Add));

        Assert.Multiple(() =>
        {
            Assert.That(result.ResponseBytes, Is.EqualTo(Encoding.UTF8.GetByteCount(csv)));
            Assert.That(result.RowsReceived, Is.EqualTo(2));
            Assert.That(result.Ttfb, Is.Not.Null);
            Assert.That(result.ContentSha256, Is.EqualTo(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(csv)))));
            Assert.That(progress, Is.Not.Empty);
            Assert.That(progress[^1].BytesRead, Is.EqualTo(result.ResponseBytes));
            Assert.That(progress[^1].RowsRead, Is.EqualTo(result.RowsReceived));
        });
    }

    private sealed class StubHandler(string csv) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/course/api/courses/export?status=PUBLISHED"));
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes(csv)),
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            return Task.FromResult(response);
        }
    }
}