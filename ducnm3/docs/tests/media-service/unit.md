# Kiểm thử đơn vị Media Service

## Phạm vi

Dự án: `backend/Services/Media/MediaService.UnitTests`
Mã nguồn: `Application/MediaCommandHandlerTests.cs`,
`Application/GetMediaContentHandlerTests.cs`,
`Application/RegisterNotificationMediaUsagesHandlerTests.cs`,
`Clients/StudentLookupClientTests.cs`,
`Endpoints/MediaRequestParserTests.cs`,
`Health/MediaDatabaseHealthProbeTests.cs` và `Storage/*.cs`
; direct upload còn có `CreateUploadIntentHandlerTests`,
`CompleteDirectUploadHandlerTests` và `MinioUploadPolicyProviderTests`.
Thành phần phụ thuộc: không gọi MinIO, MySQL, mạng hoặc Docker.

Chạy:

```bash
dotnet test backend/Services/Media/MediaService.UnitTests/MediaService.UnitTests.csproj
```

## Ca kiểm thử

| Nhóm | Ca kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- | --- |
| Sức khỏe cơ sở dữ liệu | `CheckAsyncPropagatesRequestCancellation` | Tạo `MediaDatabaseHealthProbe` với chuỗi kết nối có cổng không hợp lệ rồi hủy token trước khi gọi. | Ném `OperationCanceledException`, không trả `IsHealthy = false`. |
| Tùy chọn MinIO | `ValidatorAcceptsCompleteConfiguration` | Cung cấp điểm cuối, khóa truy cập/bí mật, năm bucket hợp lệ và khác nhau. | `MinioStorageOptionsValidator` trả thành công. |
| Tùy chọn MinIO | `ValidatorRejectsDuplicateOrMalformedBuckets` | Cấu hình bucket trùng `images` và bucket có ký tự không hợp lệ `Invalid_Bucket`. | Kiểm tra thất bại; lỗi chỉ ra các bucket phải khác nhau và bucket không hợp lệ. |
| Ánh xạ bucket | `CategoryMapsToConfiguredBucket` | Lần lượt truyền `IMAGE`, `VIDEO`, `DOCUMENT`, `AUDIO`, `OTHER`. | Trả đúng `images`, `videos`, `documents`, `audios`, `other`. |
| Khóa đối tượng | `CreateUsesUtcDateUuidAndNormalizedExtension` | Dùng `TimeProvider` cố định ở múi giờ `+07:00`, tạo khóa `png`. | Khóa dùng ngày UTC `2026/08/12`, UUID 32 ký tự hệ thập lục phân và đuôi `.png`. |
| Kiểm tra tải lên | `ValidateUploadAcceptsMatchingCategoryAndContentType` | Lần lượt kiểm tra 5 cặp loại/MIME: PNG, MP4, PDF, MPEG, octet-stream. | MIME giữ nguyên và phần mở rộng `.BIN` được chuẩn hóa thành `bin`. |
| Kiểm tra tải lên | `ValidateUploadRejectsMismatchedCategory` | Truyền loại `VIDEO` với MIME `image/png`. | Ném `StorageValidationException`. |
| Kiểm tra tải lên | `ValidateUploadRejectsUnsafeExtension` | Truyền phần mở rộng `../png`. | Ném `StorageValidationException`. |
| Kiểm tra tải lên | `ValidateUploadRejectsSizeMismatch` | Luồng có 2 byte nhưng khai báo kích thước 1. | Ném `StorageValidationException`. |
| Workflow upload | `UploadCreatesPendingBeforeStorageAndThenMarksReady` | Dùng repository/storage giả ghi lại thứ tự event. | Thứ tự là `pending -> upload -> ready`; response `READY`; actor được chuẩn hóa thành `STUDENT`. |
| Compensation upload | `UploadFailureDeletesObjectAndMarksRecordFailed` | Storage giả ném lỗi khi upload. | Thứ tự là `pending -> upload -> delete -> failed` và handler trả `MEDIA_UPLOAD_FAILED`. |
| Media usage policy | `HandleAsyncCourseThumbnailUsesReadyOriginalImageReturnsUsage` | Admin gán ảnh nguồn `READY` làm Course thumbnail. | Handler tạo usage thay thế, không yêu cầu WebP derivation. |
| Media usage policy | `HandleAsyncStudentAssignsAnotherStudentsAvatarThrowsInvalidActorType` | Student tạo avatar usage với `createdBy` khác `ownerId`. | Handler từ chối `INVALID_ACTOR_TYPE` và không ghi usage. |
| Lesson attachment | `CourseLessonAttachmentAcceptsEveryReadyOriginalMediaType` | Lần lượt gắn media gốc `READY` loại `IMAGE`, `VIDEO`, `DOCUMENT`, `AUDIO`, `OTHER` vào `COURSE/LESSON_ATTACHMENT/ATTACHMENT`. | Handler chuyển đủ năm loại media sang repository, không áp dụng ràng buộc thumbnail hoặc image-only. |
| Notification bulk usage | `HandleAsyncBatchMapsOneReferenceToEveryNotification` | Command chứa hai notification IDs và cùng một media reference. | Core assignment nhận hai usage `NOTIFICATION/NOTIFICATION_BODY/EMBED`, mỗi usage có owner ID riêng và actor `ADMIN`. |
| Notification bulk usage | `HandleAsyncBatchExceedsUsageRowLimitThrowsValidationError` | Command vượt giới hạn usage rows trong một message. | Handler trả `INVALID_MEDIA` trước khi gọi repository. |
| Notification Media Usage job | `GetNotificationMediaUsageJobStatusHandlerTests` | Job đang xử lý 600 thành công, 10 thất bại trên expected 1000. | Trả progress 61% và remaining 390; batch handler ghi đúng job ID/counter. |
| Mở rộng actor | `ActorValidationDispatchesWithoutChangingHandlers` | Đăng ký validator `STUDENT` và validator giả `ADMIN`, gửi actor viết thường `admin`. | Actor được chuẩn hóa; chỉ validator `ADMIN` được gọi, không cần đổi command handler. |
| Content handler | READY/missing/PENDING/storage failure | Dùng repository/storage stub để đọc media ở từng trạng thái và copy stream. | READY copy đúng bytes, missing trả `404`, PENDING trả `409`, storage failure trả `503`; result không public storage location. |
| URL handler | active/missing/non-image usage | `GetMediaUsageUrlHandlerTests` và `GetMediaUsageUrlsHandlerTests` dùng repository/provider giả trong memory. | Một usage trả URL, usage thiếu trả `MEDIA_USAGE_NOT_FOUND`, owner trả URL cho mọi usage và input owner sai trả `INVALID_MEDIA`. |
| Batch usage cleanup | `HandleAsync_UsageIdsProvided_DelegatesBatchCleanupToRepository` | Gửi `DeleteMediaUsagesByIdsV1` với hai usage ID tới handler. | Handler chuyển nguyên batch cho repository; repository xử lý idempotent ID đã bị xóa. |
| Multipart limit | `MultipartLengthLimitReturnsPayloadTooLarge` | Parse multipart vượt `MultipartBodyLengthLimit`. | Trả `413 PAYLOAD_TOO_LARGE`. |
| Student client | route/200/404/503 | Gọi client qua stub HTTP handler. | Dùng shared route/contract; deserialize `200`, `404 -> null`, dependency failure -> safe `503`. |
| Direct intent | validation/policy/draft | Tên file, type, size, lowercase SHA-256 và actor hợp lệ/lỗi. | Chỉ input hợp lệ tạo PENDING draft và policy exact key/type/size/checksum, trả `Content-Type` form field bắt buộc, expiry 900 giây. |
| Direct complete | verify/idempotency/concurrency | Metadata matching/mismatch, READY replay, concurrent loser và ambiguous commit. | Chỉ verified object được promotion; ETag được truyền; loser không xóa winner; replay không nhân thumbnail job. |
| Persistence mapper | `MediaPersistenceMapperTests` | Dựng `Scaffolded.MediaObject` và `Scaffolded.MediaUsage` trong memory. | Mapper tạo đúng domain entities `Media`/`MediaUsage`, gồm metadata, actor và trạng thái draft. |
| EF mapping `is_draft` | `MediaObjectIsDraftDatabaseDefaultIsTrueUsesTrueAsSentinel` | Dựng `MediaDbContext` chỉ để đọc metadata model, không mở kết nối database. | DB default và sentinel đều là `true`; khi insert `IsDraft=false`, EF gửi rõ giá trị `false` thay vì để `DEFAULT 1` ghi đè. |

Các tổ hợp sức khỏe HTTP của Media được chạy qua `TestServer`, vì vậy được ghi trong
[`component.md`](component.md), không lặp lại ở đây.

Unit tests hiện bảo vệ trình tự database-first `PENDING -> READY`, nhánh
compensation sang `FAILED`, content streaming/error mapping, shared Student
contract và việc tách request actor khỏi owner. Checksum thật,
transaction/unique index avatar và MinIO thật thuộc kiểm thử integration.
