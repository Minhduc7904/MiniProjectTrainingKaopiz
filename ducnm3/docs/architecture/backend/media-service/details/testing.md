# Media Service — Testing

## Mục đích

Media có unit, component và integration tests cho Application, endpoint, MinIO và flow.

## Kiến trúc

```mermaid
flowchart LR
  Unit --> App
  Component --> API
  Integration --> DB[(DB / MinIO)]
```

## Cách dùng

```bash
dotnet test backend/Services/Media/MediaService.UnitTests
dotnet test backend/Services/Media/MediaService.ComponentTests
dotnet test backend/Services/Media/MediaService.IntegrationTests
```

## Đã triển khai hiện tại

Các test project, direct-upload unit/component suites và
`MediaUploadUsageFlowTests`/`MinioStorageServiceTests` tồn tại. Real MinIO test
bảo vệ ETag stale khi promotion; V005 backfill và multipart draft persistence
dùng real MySQL. Chưa có một test kết hợp real MySQL + MinIO cho concurrent
complete; concurrency/finalizer hiện được chứng minh ở unit/component boundary.

## Định hướng/chưa triển khai

Không có full E2E Gateway/frontend test tại đây.

## Troubleshooting

| Hiện tượng | Cách xử lý |
| --- | --- |
| Storage test fail | Kiểm tra container/MinIO fixture và credential. |
