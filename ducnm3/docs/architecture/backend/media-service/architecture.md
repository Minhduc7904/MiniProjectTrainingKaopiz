# Kiến trúc Media Service

## Mục đích và ownership

Media Service sở hữu media objects, media usages, derivation job và toàn bộ truy cập MinIO. Xem [data model](../../../database/media-service/data-model.md). Bucket/object key không trả cho client.

~~~mermaid
flowchart LR
  Browser -->|signed POST| MinIO
  Browser --> Gateway --> Api[Media API] --> App[Application] --> Domain
  App --> Infra[Infrastructure]
  Infra --> Db[(Media database)]
  Infra --> MinIO
  MQ[RabbitMQ] --> Worker[Media Worker] --> App
~~~

## Các tầng

- **Domain:** entity, value object, hằng số và policy; không phụ thuộc framework.
- **Application:** các vertical `UseCases` cho Media, MediaUsage và MediaDerivation; repository/service port thuộc tầng này.
- **Infrastructure:** EF Core, mapper persistence, repository, transaction finalizer, MinIO, thumbnail, URL adapter, Student client, health probe và Outbox.
- **API/Worker:** Minimal API endpoint theo use case và consumer theo nghiệp vụ; không chứa business rule.

## Quy ước source

Mọi file C# trong Media Service và test bắt đầu bằng hai comment tiếng Việt: đường
dẫn repo-relative và một câu mô tả mục đích. Test kiến trúc tự động kiểm tra quy
ước này; file `Persistence/Scaffolded` ghi rõ đó là model sinh từ database.

## Tài liệu chi tiết

- [Domain](details/domain.md)
- [Application](details/application.md)
- [Infrastructure](details/infrastructure.md)
- [API](details/api.md)
- [Worker](details/worker.md)
- [Database và integration](details/database-integrations.md)
- [Testing](details/testing.md)

## Đã triển khai hiện tại

API map multipart/direct upload, usage, content URL/content stream và thumbnail
endpoint. Direct browser upload dùng public MinIO endpoint/CORS; API/Worker vẫn
dùng internal endpoint cho server-side storage. Worker đăng ký thumbnail và
notification media usage consumer; API/Worker dùng Entity Framework Outbox.
Xem [API docs](../../../api/media-service/README.md).

Media Worker sở hữu `notification_media_usage_jobs` và GET status tương ứng. Start/register/complete command dùng Notification Batch ID làm correlation; completion marker không đóng job trước khi counter chunk đạt expected count.

## Định hướng/chưa triển khai

P5-13 usage-driven draft transition và P5-20 stale/orphan cleanup là định hướng,
không phải hành vi/job đang chạy.
