# Kiểm thử đơn vị Media Service

## Phạm vi

Dự án: `backend/Services/Media/MediaService.UnitTests`
Mã nguồn: `Application/MediaCommandHandlerTests.cs`,
`Application/GetMediaContentHandlerTests.cs`,
`Application/RegisterNotificationMediaUsagesHandlerTests.cs`,
`Clients/StudentLookupClientTests.cs`,
`Endpoints/MediaRequestParserTests.cs`,
`Health/MediaDatabaseHealthProbeTests.cs` và `Storage/*.cs`
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
| Actor và owner | `UsageKeepsCreatorActorSeparateFromOwner` | Tạo avatar usage với `createdBy` khác `ownerId`. | Actor được xác minh riêng, owner được tra cứu riêng, repository nhận đúng hai ID và `ownerType=STUDENT_AVATAR`. |
| Notification bulk usage | `HandleAsyncBatchMapsOneReferenceToEveryNotification` | Command chứa hai notification IDs và cùng một media reference. | Core assignment nhận hai usage `NOTIFICATION/NOTIFICATION_BODY/EMBED`, mỗi usage có owner ID riêng và actor `ADMIN`. |
| Notification bulk usage | `HandleAsyncBatchExceedsUsageRowLimitThrowsValidationError` | Command vượt giới hạn usage rows trong một message. | Handler trả `INVALID_MEDIA` trước khi gọi repository. |
| Mở rộng actor | `ActorValidationDispatchesWithoutChangingHandlers` | Đăng ký validator `STUDENT` và validator giả `ADMIN`, gửi actor viết thường `admin`. | Actor được chuẩn hóa; chỉ validator `ADMIN` được gọi, không cần đổi command handler. |
| Content handler | READY/missing/PENDING/storage failure | Dùng repository/storage stub để đọc media ở từng trạng thái và copy stream. | READY copy đúng bytes, missing trả `404`, PENDING trả `409`, storage failure trả `503`; result không public storage location. |
| URL handler | active/missing/non-image usage | `GetMediaUsageUrlHandlerTests` và `GetMediaUsageUrlsHandlerTests` dùng repository/provider giả trong memory. | Một usage trả URL, usage thiếu trả `MEDIA_USAGE_NOT_FOUND`, owner trả URL cho mọi usage và input owner sai trả `INVALID_MEDIA`. |
| Multipart limit | `MultipartLengthLimitReturnsPayloadTooLarge` | Parse multipart vượt `MultipartBodyLengthLimit`. | Trả `413 PAYLOAD_TOO_LARGE`. |
| Student client | route/200/404/503 | Gọi client qua stub HTTP handler. | Dùng shared route/contract; deserialize `200`, `404 -> null`, dependency failure -> safe `503`. |

Các tổ hợp sức khỏe HTTP của Media được chạy qua `TestServer`, vì vậy được ghi trong
[`component.md`](component.md), không lặp lại ở đây.

Unit tests hiện bảo vệ trình tự database-first `PENDING -> READY`, nhánh
compensation sang `FAILED`, content streaming/error mapping, shared Student
contract và việc tách request actor khỏi owner. Checksum thật,
transaction/unique index avatar và MinIO thật thuộc kiểm thử integration.
