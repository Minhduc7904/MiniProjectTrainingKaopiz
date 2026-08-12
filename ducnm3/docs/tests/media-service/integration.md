# Media Service Integration Tests

## Scope

Project: `backend/Services/Media/MediaService.IntegrationTests`
Source: `Storage/MinioStorageServiceTests.cs`
Dependency: một MinIO Testcontainer cô lập; Docker Engine phải chạy. Test không
dùng MinIO trong Docker Compose của developer.

Chạy:

```bash
dotnet test backend/Services/Media/MediaService.IntegrationTests/MediaService.IntegrationTests.csproj
```

## Test fixture lifecycle

`StartMinioAsync` khởi động image MinIO cố định, xây `IMinioClient` từ endpoint
container, tạo năm bucket `images`, `videos`, `documents`, `audios`, `other`,
sau đó khởi tạo `MinioStorageService`. `StopMinioAsync` dispose client và
container sau test fixture.

## Test cases

| Test | Dữ liệu / thao tác | Pass khi |
| --- | --- | --- |
| `StorageLifecycleWorksForEachMediaCategory` — IMAGE | Upload bytes `integration-Image` với `image/png`, extension `png`; kiểm tra exists, metadata, download, delete. | Upload vào `images`; key kết thúc `.png`; size đúng; exists `true`; metadata MIME/size đúng; bytes download giống bytes upload; sau delete exists `false`. |
| `StorageLifecycleWorksForEachMediaCategory` — VIDEO | Cùng vòng đời với `video/mp4`, extension `mp4`. | Bucket `videos`, metadata và dữ liệu download/delete đúng như contract. |
| `StorageLifecycleWorksForEachMediaCategory` — DOCUMENT | Cùng vòng đời với `application/pdf`, extension `pdf`. | Bucket `documents`, metadata và dữ liệu download/delete đúng như contract. |
| `StorageLifecycleWorksForEachMediaCategory` — AUDIO | Cùng vòng đời với `audio/mpeg`, extension `mp3`. | Bucket `audios`, metadata và dữ liệu download/delete đúng như contract. |
| `StorageLifecycleWorksForEachMediaCategory` — OTHER | Cùng vòng đời với `application/octet-stream`, extension `bin`. | Bucket `other`, metadata và dữ liệu download/delete đúng như contract. |
| `HealthProbeIsHealthyWhenAllBucketsExist` | Gọi `IStorageHealthProbe.CheckAsync` sau khi fixture tạo đủ năm bucket. | `IsHealthy = true`. |

Các test lifecycle bắt buộc object key do storage service sinh ra, không cho test
tự chọn bucket hay object key. Điều này bảo vệ mapping category → bucket và
contract stream của adapter.
