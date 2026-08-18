// File: backend/Services/Notification/NotificationService.Infrastructure/Clients/Student/StudentRecipientClient.cs
// Mục đích: Triển khai client tích hợp hệ thống ngoài cho StudentRecipientClient.

using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using NotificationService.Application;
using NotificationService.Application.Abstractions;

namespace NotificationService.Infrastructure.Clients.Student;

public sealed class StudentRecipientClient(HttpClient httpClient) : IStudentRecipientClient
{
    public async IAsyncEnumerable<IReadOnlyList<Guid>> GetActiveStudentIdPagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var page = 1; ; page++)
        {
            var response = await GetPageAsync(page, cancellationToken);
            yield return response.StudentIds;
            if (page >= response.TotalPages) yield break;
        }
    }

    private async Task<StudentRecipientPage> GetPageAsync(int page, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync($"{ApiRoutes.Students.ListServicePath()}?status=ACTIVE&page={page}&pageSize=100", cancellationToken);
            if (!response.IsSuccessStatusCode) throw NotificationErrors.StudentServiceUnavailable();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
            var ids = new List<Guid>();
            foreach (var item in document.RootElement.GetProperty("data").EnumerateArray())
            {
                if (Guid.TryParse(item.GetProperty("id").GetString(), out var id)) ids.Add(id);
            }

            var totalPages = document.RootElement
                .GetProperty("meta")
                .GetProperty("pagination")
                .GetProperty("totalPages")
                .GetInt32();
            return new StudentRecipientPage(ids, totalPages);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (NotificationApplicationException) { throw; }
        catch (Exception) { throw NotificationErrors.StudentServiceUnavailable(); }
    }

    private sealed record StudentRecipientPage(IReadOnlyList<Guid> StudentIds, int TotalPages);
}
