# Kiến trúc Media Service

## Mục đích và ownership

Media Service sở hữu media objects, media usages, derivation job và toàn bộ truy cập MinIO. Xem [data model](../../../database/media-service/data-model.md). Bucket/object key không trả cho client.

~~~mermaid
flowchart LR
  Gateway --> Api[Media API] --> App[Application] --> Domain
  App --> Infra[Infrastructure]
  Infra --> Db[(Media database)]
  Infra --> MinIO
  MQ[RabbitMQ] --> Worker[Media Worker] --> App
~~~

## Các tầng

- **Domain:** media, usage và actor policy.
- **Application:** upload, content, URL, usage và storage/URL abstraction.
- **Infrastructure:** EF Core, MinIO adapter, Student query client, health probe, Outbox.
- **API/Worker:** multipart/content endpoint; thumbnail và notification-media consumer.

## Tài liệu chi tiết

- [Domain](details/domain.md)
- [Application](details/application.md)
- [Infrastructure](details/infrastructure.md)
- [API](details/api.md)
- [Worker](details/worker.md)
- [Database và integration](details/database-integrations.md)
- [Testing](details/testing.md)

## Đã triển khai hiện tại

API map upload, usage, content URL/content stream và thumbnail endpoint. Worker đăng ký thumbnail và notification media usage consumer; API/Worker dùng Entity Framework Outbox. Xem [API docs](../../../api/media-service/README.md).

## Định hướng/chưa triển khai

Stale PENDING cleanup bởi Scheduler là định hướng, không phải job đang chạy.
