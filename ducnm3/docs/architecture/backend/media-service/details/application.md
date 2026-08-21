# Media Service — Application

## Mục đích

Application điều phối multipart/direct upload, completion, content, usage URL, thumbnail, actor validation và media usage assignment policy qua
storage/persistence/client abstractions.

## Kiến trúc

```mermaid
flowchart LR
  API --> App[UseCases / handlers]
  App --> Ports[Storage, persistence, Student lookup]
  App --> Msg[Thumbnail command]
  App --> Policy[Signed upload policy port]
```

## Cách dùng

API/Worker inject handler; `MediaUsageAssignmentPolicy` tập trung rule tuple
owner/usage, actor và thuộc tính media trước khi handler chọn operation lưu usage.
`IMediaRepository` chỉ sở hữu media object,
`IMediaUsageRepository` sở hữu liên kết usage, còn `IStorage`, `IStudentLookup`
và `IMediaUrlProvider` là các port khác được Infrastructure hiện thực.

Media API có query nội bộ lấy active usage ID theo nhiều owner scope; Media Worker nhận `DeleteMediaUsagesByIdsV1` để soft-delete batch và đưa Media không còn usage active về draft.

## Đã triển khai hiện tại

Có domain entities `Media` và `MediaUsage`, `CreateUploadIntent`,
`CompleteDirectUpload`, actor validation, storage/persistence/url abstractions và
`GenerateMediaThumbnailV1` contract.

## Định hướng/chưa triển khai

Không coi usage-driven draft transition hoặc stale/orphan cleanup là Application
job đang chạy; lần lượt thuộc P5-13 và P5-20.

## Troubleshooting

| Hiện tượng | Cách xử lý |
| --- | --- |
| Handler gọi MinIO trực tiếp | Inject `IStorage`, không tham chiếu SDK. |
