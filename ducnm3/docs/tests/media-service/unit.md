# Media Service Unit Tests

## Scope

Project: `backend/Services/Media/MediaService.UnitTests`
Sources: `Health/MediaDatabaseHealthProbeTests.cs` and `Storage/*.cs`
Dependency: không gọi MinIO, MySQL, network, hoặc Docker.

Chạy:

```bash
dotnet test backend/Services/Media/MediaService.UnitTests/MediaService.UnitTests.csproj
```

## Test cases

| Nhóm | Test case | Setup và thao tác | Pass khi |
| --- | --- | --- | --- |
| Database health | `CheckAsyncPropagatesRequestCancellation` | Tạo `MediaDatabaseHealthProbe` với connection string cổng không hợp lệ rồi huỷ token trước khi gọi. | Ném `OperationCanceledException`, không trả `IsHealthy = false`. |
| MinIO options | `ValidatorAcceptsCompleteConfiguration` | Cung cấp endpoint, access/secret key, năm bucket hợp lệ và khác nhau. | `MinioStorageOptionsValidator` trả thành công. |
| MinIO options | `ValidatorRejectsDuplicateOrMalformedBuckets` | Cấu hình bucket trùng `images` và bucket có ký tự không hợp lệ `Invalid_Bucket`. | Validation fail; lỗi chỉ ra bucket phải khác nhau và bucket không hợp lệ. |
| Bucket mapping | `CategoryMapsToConfiguredBucket` | Lần lượt truyền `IMAGE`, `VIDEO`, `DOCUMENT`, `AUDIO`, `OTHER`. | Trả đúng `images`, `videos`, `documents`, `audios`, `other`. |
| Object key | `CreateUsesUtcDateUuidAndNormalizedExtension` | Dùng `TimeProvider` cố định ở múi giờ `+07:00`, tạo key `png`. | Key dùng ngày UTC `2026/08/12`, UUID 32 ký tự hex và đuôi `.png`. |
| Upload validation | `ValidateUploadAcceptsMatchingCategoryAndContentType` | Lần lượt kiểm tra 5 cặp category/MIME: PNG, MP4, PDF, MPEG, octet-stream. | MIME giữ nguyên và extension `.BIN` được chuẩn hoá thành `bin`. |
| Upload validation | `ValidateUploadRejectsMismatchedCategory` | Truyền category `VIDEO` với MIME `image/png`. | Ném `StorageValidationException`. |
| Upload validation | `ValidateUploadRejectsUnsafeExtension` | Truyền extension `../png`. | Ném `StorageValidationException`. |
| Upload validation | `ValidateUploadRejectsSizeMismatch` | Stream có 2 bytes nhưng khai báo size 1. | Ném `StorageValidationException`. |

Media health HTTP combinations được chạy qua `TestServer`, vì vậy được ghi trong
[`component.md`](component.md), không lặp lại ở đây.
