# `POST /media/api/media` — Upload media

API contract: [`post-media.md`](../../api/media-service/endpoints/post-media.md)

## Mục tiêu

Học viên upload file qua Gateway; Media Service lưu metadata và object, sau đó
trả media `READY` cùng content URL công khai.

## Actor và thành phần

- Học viên/client.
- API Gateway.
- Media Service.
- Student Service.
- MySQL Media, MinIO, RabbitMQ và Media Worker.

## Điều kiện trước

- Multipart có `file`, `mediaType`, `uploadedByType`, `uploadedBy`.
- Actor type được hỗ trợ và actor tồn tại trong Student Service.
- MIME, extension và kích thước file hợp lệ.

## UML luồng chạy

```mermaid
sequenceDiagram
    participant Client
    participant Gateway
    participant API as Media Service
    participant Student as Student Service
    participant DB as MySQL Media
    participant Storage as MinIO
    participant Outbox
    participant MQ as RabbitMQ
    participant Worker as Media Worker

    Client->>Gateway: POST /media/api/media (multipart)
    Gateway->>API: Forward upload
    API->>Student: Verify uploadedBy actor
    Student-->>API: Actor exists
    API->>DB: Insert source media PENDING
    API->>Storage: Stream upload + SHA-256
    Storage-->>API: Upload complete
    API->>DB: Mark source READY; create thumbnail job/outbox if supported
    API-->>Gateway: 201 source media + thumbnail QUEUED
    Gateway-->>Client: Created response
    Outbox->>MQ: Publish GenerateMediaThumbnail
    MQ->>Worker: Deliver command
    Worker->>Storage: Download source, create WebP, upload thumbnail
    Worker->>DB: Mark thumbnail job/media READY
```

## Luồng chính

1. Client gửi multipart tới Gateway.
2. Media Service validate request và tra cứu actor qua Student Service.
3. Application cấp phát storage location và ghi `media_objects=PENDING`.
4. MinIO adapter stream object và tính SHA-256.
5. Repository cập nhật media gốc `READY`. Với ảnh, video và PDF, cùng
   transaction tạo thumbnail media `PENDING`, derivation job `QUEUED` và
   Outbox command.
6. API trả `201`, `Location`, Gateway `contentUrl` và trạng thái thumbnail;
   bucket/object key không
   xuất hiện trong response.
7. Outbox chuyển command tới RabbitMQ; Media Worker download file gốc, tạo
   raster, fit 640×640, encode WebP và upload vào bucket `images`.
8. Worker chuyển thumbnail/job sang `READY` và tạo thumbnail usage nếu chưa có.

## Luồng lỗi

- Request/MIME/actor sai: `400`, `404` hoặc `415`.
- Payload vượt giới hạn: `413 PAYLOAD_TOO_LARGE`.
- Student Service hoặc MinIO lỗi: `503`.
- Upload lỗi được compensation best effort bằng xóa object và chuyển `FAILED`.
- Lỗi thumbnail không làm media gốc thất bại. RabbitMQ retry; khi hết retry job
  ở `FAILED` và có thể được enqueue lại bằng API retry.

## Dữ liệu và side effects

- Tạo một media gốc; với type hỗ trợ còn tạo media dẫn xuất và derivation job.
- Tạo object MinIO trong bucket theo media category.
- Tạo Outbox/Inbox state để delivery và consumer idempotent.
- Không thay đổi database Student.
- Upload không idempotent; mỗi lần thành công tạo media mới.

## Test mapping

- Unit: validation, thứ tự `PENDING -> READY`, compensation.
- Component: multipart, envelope, `contentUrl`, error mapping.
- Integration: MySQL migration/repository và MinIO object/checksum thật.
