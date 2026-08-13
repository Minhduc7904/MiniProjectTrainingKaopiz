# Kiểm thử tích hợp Media Service

## Phạm vi

Dự án: `backend/Services/Media/MediaService.IntegrationTests`
Mã nguồn: `Storage/MinioStorageServiceTests.cs`
Thành phần phụ thuộc: một Testcontainer MinIO cô lập; Docker Engine phải chạy.
Kiểm thử không dùng MinIO trong Docker Compose của lập trình viên.

Chạy:

```bash
dotnet test backend/Services/Media/MediaService.IntegrationTests/MediaService.IntegrationTests.csproj
```

## Vòng đời bộ kiểm thử

`StartMinioAsync` khởi động image MinIO cố định, tạo `IMinioClient` từ điểm cuối
container, tạo năm bucket `images`, `videos`, `documents`, `audios`, `other`,
sau đó khởi tạo `MinioStorageService`. `StopMinioAsync` giải phóng máy khách và
container sau bộ kiểm thử.

## Ca kiểm thử

| Kiểm thử | Dữ liệu / thao tác | Đạt khi |
| --- | --- | --- |
| `StorageLifecycleWorksForEachMediaCategory` — IMAGE | Tải lên các byte `integration-Image` với `image/png`, phần mở rộng `png`; kiểm tra tồn tại, siêu dữ liệu, tải xuống và xóa. | Tải lên `images`; khóa kết thúc bằng `.png`; kích thước đúng; trạng thái tồn tại là `true`; MIME/kích thước trong siêu dữ liệu đúng; các byte tải xuống giống byte tải lên; sau khi xóa, trạng thái tồn tại là `false`. |
| `StorageLifecycleWorksForEachMediaCategory` — VIDEO | Cùng vòng đời với `video/mp4`, phần mở rộng `mp4`. | Bucket `videos`, siêu dữ liệu và dữ liệu tải xuống/xóa đúng theo hợp đồng. |
| `StorageLifecycleWorksForEachMediaCategory` — DOCUMENT | Cùng vòng đời với `application/pdf`, phần mở rộng `pdf`. | Bucket `documents`, siêu dữ liệu và dữ liệu tải xuống/xóa đúng theo hợp đồng. |
| `StorageLifecycleWorksForEachMediaCategory` — AUDIO | Cùng vòng đời với `audio/mpeg`, phần mở rộng `mp3`. | Bucket `audios`, siêu dữ liệu và dữ liệu tải xuống/xóa đúng theo hợp đồng. |
| `StorageLifecycleWorksForEachMediaCategory` — OTHER | Cùng vòng đời với `application/octet-stream`, phần mở rộng `bin`. | Bucket `other`, siêu dữ liệu và dữ liệu tải xuống/xóa đúng theo hợp đồng. |
| `HealthProbeIsHealthyWhenAllBucketsExist` | Gọi `IStorageHealthProbe.CheckAsync` sau khi bộ kiểm thử tạo đủ năm bucket. | `IsHealthy = true`. |

Các kiểm thử vòng đời bắt buộc dùng khóa đối tượng do dịch vụ lưu trữ sinh ra,
không cho kiểm thử tự chọn bucket hay khóa đối tượng. Điều này bảo vệ ánh xạ loại
→ bucket và hợp đồng luồng của adapter.
