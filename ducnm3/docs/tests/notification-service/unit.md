# Kiểm thử đơn vị Notification Service

## Phạm vi

Dự án: `backend/Services/Notification/NotificationService.UnitTests`
Mã nguồn: `UnitTest1.cs`, `BatchNotificationTests.cs`, `NotificationBatchEndpointTests.cs`, `NotificationMediaReferenceExtractorTests.cs`
Thành phần phụ thuộc: không có MySQL hoặc mạng.

Chạy:

```bash
dotnet test backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `CheckAsyncPropagatesRequestCancellation` | Tạo `NotificationDatabaseHealthProbe` với chuỗi kết nối có cổng không hợp lệ; hủy token trước khi gọi `CheckAsync`. | Trình kiểm tra ném `OperationCanceledException`; thao tác hủy yêu cầu không bị bỏ qua hoặc ánh xạ thành `IsHealthy = false`. |

Kiểm thử hiện tại chỉ bảo vệ hành vi hủy của trình kiểm tra sức khỏe cơ sở dữ liệu. Chưa
có kiểm thử tích hợp MySQL hoặc RabbitMQ trong dự án này.

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `HandleAsync_RejectsScopeOtherThanAllStudents` | Gọi Create handler với `COURSE_ENROLLED`. | Trả validation 400 trước khi gọi Student Service. |
| `HandleAsync_CreatesPendingBatchAndQueuesSnapshotCommand` | Create handler với batch hợp lệ. | Chỉ tạo batch `PENDING` và phát `SnapshotNotificationBatchV1`; không gọi Student Service trong HTTP path. |
| `HandleAsync_RecipientsStreamed_PersistsPagesAndQueuesDispatch` | Snapshot handler dùng Student client giả trả hai trang. | Lưu từng trang và phát `DispatchNotificationBatchV1` sau khi snapshot hoàn tất. |
| `HandleAsync_StudentServiceUnavailable_MarksBatchFailed` | Student client giả ném `STUDENT_SERVICE_UNAVAILABLE`. | Batch được đánh dấu `FAILED`, không phát dispatch command. |
| `FakeNotificationSenderTests` | Sinh UUID có hash thỏa từng rule. | Lần một/lần hai thất bại đúng điều kiện `% 20`/`% 100`. |
| `HandleAsync_FirstBusinessFailureMarksItemForRetryAndRequeues` | Sender giả ném lỗi ở lần gửi đầu. | Item được đánh dấu thất bại nghiệp vụ và command được phát lại. |
| `NotificationBatchEndpointTests` | TestServer map POST/GET cùng doubles in-memory. | POST trả 202 + Location, GET trả 200, scope lạ trả 400. |
| `GetFailedItems_BatchExists_ReturnsCursorEnvelope` | TestServer map GET lỗi theo batch có summary in-memory. | Trả `200`, `Cache-Control: no-store`, data `items` rỗng và cursor pagination envelope. |
| `ExtractEmbeddedAndAttachedContentUrlsReturnsDistinctReferences` | Parse image và link Markdown dùng Media `contentUrl`. | Sinh lần lượt `EMBED`/`ATTACHMENT` và bỏ cặp trùng. |
| `ExtractExternalMarkdownLinkThrowsValidationError` | Parse Markdown link không phải `contentUrl` Media Service. | Trả validation 400 trước khi tạo notification hoặc batch. |

Lưu ý: `NotificationBatchEndpointTests` là TestServer component coverage cũ nằm
trong project UnitTests. Endpoint `failed-items` mới cần được chuyển vào project
ComponentTests khi tách project kiểm thử Notification Service theo quy ước mới.
