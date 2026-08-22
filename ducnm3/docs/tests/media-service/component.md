# Kiểm thử thành phần Media Service

## Phạm vi

Dự án legacy: `backend/Services/Media/MediaService.UnitTests` với
`Endpoints/MediaHealthEndpointTests.cs` và `Endpoints/MediaCommandEndpointTests.cs`.

Dự án URL usage: `backend/Services/Media/MediaService.ComponentTests`
với `Endpoints/GetMediaUsageUrlEndpointsComponentTests.cs` và
`Endpoints/DirectMediaUploadEndpointsComponentTests.cs`.
Thành phần phụ thuộc: ASP.NET Core `TestServer`; các trình kiểm tra cơ sở dữ liệu và lưu
trữ đều là đối tượng giả trong bộ nhớ.

Chạy:

```bash
dotnet test backend/Services/Media/MediaService.UnitTests/MediaService.UnitTests.csproj
dotnet test backend/Services/Media/MediaService.ComponentTests/MediaService.ComponentTests.csproj
```

## Ca kiểm thử

`GetMediaSummaryEndpointComponentTests` gửi `GET /api/media/summary` với actor ADMIN; đạt khi trả `200`, `totalMedia`, envelope chuẩn và `Cache-Control: no-store`.

`GetMediaLibraryEndpointComponentTests` gửi `GET /api/media/library` với actor
ADMIN, `mediaType=IMAGE` và `status=pending`; đạt khi route bind đúng filter,
handler nhận `PENDING` đã normalize và trả `200` envelope an toàn.

`HealthEndpointReportsEachDependencyCombination` chạy bốn ca dựa trên dữ liệu:

| Trình kiểm tra cơ sở dữ liệu | Trình kiểm tra lưu trữ | HTTP dự kiến | Đạt khi phản hồi |
| --- | --- | --- | --- |
| healthy | healthy | `200` | Có `data.status = healthy`, `data.database.status = healthy`, và `data.storage.status = healthy`. |
| unhealthy | healthy | `503` | Vỏ lỗi dùng chung có `error.code = DATABASE_UNAVAILABLE`. |
| healthy | unhealthy | `503` | Vỏ lỗi dùng chung có `error.code = STORAGE_UNAVAILABLE`. |
| unhealthy | unhealthy | `503` | Vỏ lỗi dùng chung có `error.code = DEPENDENCY_UNAVAILABLE`. |

Kiểm thử xác nhận điểm cuối chạy với hai kết quả thành phần phụ thuộc đã biết và
chọn đúng hợp đồng API. Kiểm thử không mở MySQL/MinIO thật; vòng đời lưu trữ thật
thuộc kiểm thử tích hợp.

| Kiểm thử | Request | Đạt khi |
| --- | --- | --- |
| `MultipartUploadAndUsageReturnStandardCreatedResponses` | Gửi multipart có `file`, `mediaType`, `uploadedByType`, `uploadedBy`, sau đó gửi JSON usage với actor khác `ownerId`. | Cả hai endpoint trả `201` trong response envelope chuẩn; upload trả `READY` nhưng không lộ bucket/object key; usage lưu riêng actor và owner. |
| `UnknownActorTypeReturnsSafeValidationError` | Gửi multipart với `uploadedByType=ADMIN` khi chưa đăng ký Admin validator. | Trả `400 INVALID_ACTOR_TYPE` và response không lộ stack trace. |
| `UploadedMediaContentStreamsWithSafeHeaders` | Upload media rồi gọi content route từ `contentUrl`. | Trả đúng bytes, MIME, `Content-Length`, `Cache-Control: no-store`; response không lộ bucket/object key. |

Các dependency trong component test dùng shared test doubles tại `TestDoubles/`.
Checksum, MySQL,
MinIO và transaction thay active avatar được bao phủ trong
[`integration.md`](integration.md).

| Kiểm thử | Request | Đạt khi |
| --- | --- | --- |
| `GetUsageExistsReturnsOkEnvelope` | `GET /api/media/usages/{usageId}/url` | `200`, JSON envelope và URL do provider giả sinh. |
| `GetOwnerUsagesReturnsEveryUrlInEnvelope` | `GET /api/media/usages/urls` với owner query | `200` và mảng URL có đúng thứ tự `displayOrder`. |
| `GetUsageMissingReturnsNotFoundEnvelope` | Usage không tồn tại | `404 MEDIA_USAGE_NOT_FOUND` trong envelope an toàn. |
| `PostThumbnailDerivativeUsageReturnsInvalidMediaEnvelope` | `POST /api/media/usages` với thumbnail derivative và tuple nội bộ. | `400 INVALID_MEDIA`; direct API không cho client tạo `MEDIA/MEDIA_THUMBNAIL/THUMBNAIL`. |
| `CreateIntentReturnsCreatedEnvelopeAndCanonicalLocation` | JSON metadata direct upload hợp lệ | `201`, `Location=/media/api/media/{id}`, PENDING draft và signed response qua envelope. |
| `CompleteReturnsReadyDraftAndIsRetrySafe` | Media PENDING/READY với storage/finalizer doubles | `200` READY draft; replay giữ established result và không nhân side effect. |
| Direct errors | actor/media/object invalid | `400/404/409` trong envelope an toàn; không lộ storage routing/signed fields. |
| `GetNotificationMediaUsageJobStatusEndpointComponentTests` | GET job status qua route Media-owned. | Trả `200`, progress/remaining đúng và `Cache-Control: no-store`. |
| `GetMediaBackgroundJobsEndpointComponentTests` | `GET /api/media/jobs` với actor header ADMIN và repository double. | Trả `200`, offset envelope, item projection an toàn và `Cache-Control: no-store`. |
