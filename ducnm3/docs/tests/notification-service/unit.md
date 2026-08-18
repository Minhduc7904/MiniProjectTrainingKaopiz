# Kiểm thử đơn vị Notification Service

## Phạm vi

Dự án: `backend/Services/Notification/NotificationService.UnitTests`
Mã nguồn được tổ chức theo `Architecture/`, `Domain/Entities/`,
`Application/UseCases/` và `Infrastructure/Services/` để phản chiếu production code.
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
| `HandleAsync_ConcurrentDispatchConfigured_QueuesOneCommandPerSlot` | Snapshot hoàn tất với `DispatchChunkConcurrency=3`. | Phát đúng ba dispatch command để duy trì ba slot chunk. |
| `HandleAsync_StudentServiceUnavailable_MarksBatchFailed` | Student client giả ném `STUDENT_SERVICE_UNAVAILABLE`. | Batch được đánh dấu `FAILED`, không phát dispatch command. |
| `FakeNotificationSenderTests` | Sinh UUID có hash thỏa từng rule. | Lần một/lần hai thất bại đúng điều kiện `% 20`/`% 100`. |
| `HandleAsync_FirstBusinessFailureMarksItemForRetryAndRequeues` | Sender giả ném lỗi ở lần gửi đầu. | Item được đánh dấu thất bại nghiệp vụ và command được phát lại. |
| `HandleAsyncSuccessfulChunkQueuesOneMediaUsageBatchCommand` | Hai item cùng bodyMarkdown media được gửi thành công trong một chunk. | Phát đúng một `RegisterNotificationMediaUsageBatchV1` chứa hai notification IDs; không phát command theo từng item. |
| `ExtractEmbeddedAndAttachedContentUrlsReturnsDistinctReferences` | Parse image và link Markdown dùng Media `contentUrl`. | Sinh lần lượt `EMBED`/`ATTACHMENT` và bỏ cặp trùng. |
| `ExtractExternalMarkdownLinkThrowsValidationError` | Parse Markdown link không phải `contentUrl` Media Service. | Trả validation 400 trước khi tạo notification hoặc batch. |
| `NotificationBatchStateTests` | Áp dụng snapshot, counter, retry và finalize trên Domain entity thuần. | Status/counter đúng hợp đồng hiện tại, không cần EF Core. |
| `NotificationArchitectureTests` | Quét namespace, assembly reference và cây endpoint. | Không quay lại namespace cũ hoặc dependency ngược layer; mỗi route vẫn có file riêng. |
| `NotificationSourceFileHeaderTests` | Quét toàn bộ source C# của Notification Service. | Mỗi file có đúng path và mô tả trách nhiệm tiếng Việt ở hai dòng đầu. |

TestServer endpoint coverage đã được chuyển sang project
`NotificationService.ComponentTests`; xem tài liệu kiến trúc testing để chạy riêng.
