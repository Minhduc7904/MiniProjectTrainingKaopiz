// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/GetFailedItems/GetNotificationBatchFailedItemsHandler.cs
// Mục đích: Điều phối use case GetNotificationBatchFailedItemsHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using NotificationService.Application.Abstractions;

namespace NotificationService.Application.Features.Batches.GetFailedItems;

public sealed class GetNotificationBatchFailedItemsHandler(INotificationBatchRepository repository)
{
    public async Task<NotificationBatchFailedItemsPage> HandleAsync(Guid batchId, string? cursor, int limit, CancellationToken cancellationToken)
    {
        if (batchId == Guid.Empty)
        {
            throw NotificationErrors.Validation("batchId must be a valid UUID.");
        }

        if (limit is < 1 or > 100)
        {
            throw NotificationErrors.Validation("limit must be between 1 and 100.");
        }

        if (await repository.GetByIdAsync(batchId, cancellationToken) is null)
        {
            throw NotificationErrors.BatchNotFound();
        }

        return await repository.GetFailedItemsAsync(batchId, DecodeCursor(cursor), limit, cancellationToken);
    }

    private static Guid? DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return null;

        try
        {
            var value = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return Guid.TryParse(value, out var itemId) ? itemId : throw new FormatException();
        }
        catch (FormatException)
        {
            throw NotificationErrors.Validation("cursor is invalid.");
        }
    }

    public static string? EncodeCursor(Guid? itemId) => itemId is null
        ? null
        : Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(itemId.Value.ToString("D")));
}
