using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using NotificationService.Application;
using NotificationService.Application.Abstractions;

namespace NotificationService.Infrastructure.Clients.Student;

public sealed class StudentRecipientClient(HttpClient httpClient) : IStudentRecipientClient
{
    public async Task<IReadOnlyList<Guid>> GetAllActiveStudentIdsAsync(CancellationToken cancellationToken)
    {
        var ids = new List<Guid>();
        for (var page = 1; ; page++)
        {
            try
            {
                using var response = await httpClient.GetAsync($"{ApiRoutes.Students.ListServicePath()}?status=ACTIVE&page={page}&pageSize=100", cancellationToken);
                if (!response.IsSuccessStatusCode) throw NotificationErrors.StudentServiceUnavailable();
                using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
                foreach (var item in document.RootElement.GetProperty("data").EnumerateArray())
                {
                    if (Guid.TryParse(item.GetProperty("id").GetString(), out var id)) ids.Add(id);
                }

                var pagination = document.RootElement.GetProperty("meta").GetProperty("pagination");
                if (page >= pagination.GetProperty("totalPages").GetInt32()) return ids;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (NotificationApplicationException) { throw; }
            catch (Exception) { throw NotificationErrors.StudentServiceUnavailable(); }
        }
    }
}
