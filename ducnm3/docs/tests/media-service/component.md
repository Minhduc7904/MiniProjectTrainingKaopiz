# Kiểm thử thành phần Media Service

## Phạm vi

Dự án: `backend/Services/Media/MediaService.UnitTests`
Mã nguồn: `Endpoints/MediaHealthEndpointTests.cs` và
`Endpoints/MediaCommandEndpointTests.cs`
Thành phần phụ thuộc: ASP.NET Core `TestServer`; các trình kiểm tra cơ sở dữ liệu và lưu
trữ đều là đối tượng giả trong bộ nhớ.

Chạy:

```bash
dotnet test backend/Services/Media/MediaService.UnitTests/MediaService.UnitTests.csproj
```

## Ca kiểm thử

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

Các dependency trong component test đều là fake trong memory. Checksum, MySQL,
MinIO và transaction thay active avatar được bao phủ trong
[`integration.md`](integration.md).
