# Kiểm thử tích hợp Media Service

## Phạm vi

Dự án: `backend/Services/Media/MediaService.IntegrationTests`
Mã nguồn: `Storage/MinioStorageServiceTests.cs` và
`Flows/MediaUploadUsageFlowTests.cs`
Thành phần phụ thuộc: các Testcontainer MinIO và MySQL 8.4 cô lập; Docker Engine
phải chạy.
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

Flow fixture khởi động MySQL 8.4 và MinIO riêng, áp dụng migration Media thật,
tạo năm bucket dành cho test rồi chạy Application handler với
`EfMediaRepository` và `MinioStorageService` thật. Student lookup được thay bằng
stub trong bộ nhớ để test không phụ thuộc Student Service qua mạng.

## Ca kiểm thử

| Kiểm thử | Dữ liệu / thao tác | Đạt khi |
| --- | --- | --- |
| `StorageLifecycleWorksForEachMediaCategory` — IMAGE | Tải lên các byte `integration-Image` với `image/png`, phần mở rộng `png`; kiểm tra tồn tại, siêu dữ liệu, tải xuống và xóa. | Tải lên `images`; khóa kết thúc bằng `.png`; kích thước đúng; trạng thái tồn tại là `true`; MIME/kích thước trong siêu dữ liệu đúng; các byte tải xuống giống byte tải lên; sau khi xóa, trạng thái tồn tại là `false`. |
| `StorageLifecycleWorksForEachMediaCategory` — VIDEO | Cùng vòng đời với `video/mp4`, phần mở rộng `mp4`. | Bucket `videos`, siêu dữ liệu và dữ liệu tải xuống/xóa đúng theo hợp đồng. |
| `StorageLifecycleWorksForEachMediaCategory` — DOCUMENT | Cùng vòng đời với `application/pdf`, phần mở rộng `pdf`. | Bucket `documents`, siêu dữ liệu và dữ liệu tải xuống/xóa đúng theo hợp đồng. |
| `StorageLifecycleWorksForEachMediaCategory` — AUDIO | Cùng vòng đời với `audio/mpeg`, phần mở rộng `mp3`. | Bucket `audios`, siêu dữ liệu và dữ liệu tải xuống/xóa đúng theo hợp đồng. |
| `StorageLifecycleWorksForEachMediaCategory` — OTHER | Cùng vòng đời với `application/octet-stream`, phần mở rộng `bin`. | Bucket `other`, siêu dữ liệu và dữ liệu tải xuống/xóa đúng theo hợp đồng. |
| `HealthProbeIsHealthyWhenAllBucketsExist` | Gọi `IStorageHealthProbe.CheckAsync` sau khi bộ kiểm thử tạo đủ năm bucket. | `IsHealthy = true`. |
| `UploadPersistsReadyChecksumAndUsageReplacesStudentAvatar` | Upload ảnh A và B bằng handler thật; gán avatar A → B → A trên MySQL đã áp dụng V003, rồi gửi lại A khi A đang active. | Media là `READY`; checksum đúng; có ba usage history nhưng chỉ một active trỏ lại A; soft-deleted reference được tái sử dụng; duplicate active trả `MEDIA_USAGE_CONFLICT`. |
| `V005BackfillsExistingMediaDraftStateAndCreatesIndex` | Chèn legacy media trước V005: có active usage, không usage và PENDING; apply migration thật. | Active usage non-draft/null; còn lại draft với timestamp deterministic; `ix_media_objects_draft_cleanup` tồn tại. |
| `PromoteAsyncSourceChangesAfterHeadRejectsStaleEtag` | Upload source lên real MinIO, stat ETag, thay source rồi promote với ETag cũ. | Real MinIO từ chối copy theo stale ETag; final object không được coi là committed. |
| `V006TracksNotificationMediaUsageJobUntilExpectedCountCompletes` | Apply migration thật; start job, ghi 40/100 rồi ghi thêm 60. | Job giữ `PROCESSING` khi chưa đủ expected count và chuyển `COMPLETED` khi đạt 100. |
| `GetActiveUsageIdsByOwnersAsyncMultipleOwnerScopesReturnsOnlyExactActiveUsageIds` | Thêm ba usage vào MySQL Testcontainer, trong đó một usage có cùng `ownerId` nhưng owner service/type khác. | Repository chỉ trả usage active khớp chính xác từng owner scope, phục vụ batch cleanup khi xóa Course. |
| Thumbnail derivation internal usage | Worker hoàn tất thumbnail WebP với source original thật. | Chỉ Worker tạo được `MEDIA/MEDIA_THUMBNAIL/THUMBNAIL`, `ownerId` đúng bằng ID original và thumbnail không được dùng ở usage nghiệp vụ. |

Các kiểm thử vòng đời dùng `MinioStorageLocationAllocator` để reserve
bucket/object key trước khi gọi storage adapter. Điều này bảo vệ ánh xạ loại →
bucket và hợp đồng DB-first của flow.

Flow integration bảo vệ database-first upload ở kết quả bền vững, checksum thật
và transaction thay avatar/active uniqueness. Nhánh lỗi/compensation được kiểm
tra bằng unit test; Media endpoint được kiểm tra qua `TestServer`. Gateway
pass-through và cleanup `PENDING` stale chưa có automated test. Hiện chưa có
combined real MySQL+MinIO concurrency test cho direct complete; không suy diễn
coverage này từ unit/component test.
