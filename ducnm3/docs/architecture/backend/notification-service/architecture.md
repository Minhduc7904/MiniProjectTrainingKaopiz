# Kiến trúc Notification Service

## Mục đích và ownership

Service sở hữu notification đơn, batch, batch item, inbox/read state và MassTransit persistence. Xem [data model](../../../database/notification-service/data-model.md). Recipient Student/Course là logical reference; media usage đi qua messaging boundary.

~~~mermaid
flowchart LR
  Gateway --> Api[Notification API] --> App[Application] --> Domain
  App --> Infra[Infrastructure] --> Db[(Notification database)]
  MQ[RabbitMQ] --> Worker[Notification Worker] --> App
  App -. query .-> Student[Student Service]
  App -. command .-> Media[Media Service]
~~~

## Các tầng

- **Domain:** notification và batch state.
- **Application:** create/read/query batch, recipient và messaging abstraction.
- **Infrastructure:** EF Core, Student client, sending adapter và Outbox.
- **API/Worker:** HTTP endpoint và snapshot/dispatch command consumer.

## Tài liệu chi tiết

- [Domain](details/domain.md)
- [Application](details/application.md)
- [Infrastructure](details/infrastructure.md)
- [API](details/api.md)
- [Worker](details/worker.md)
- [Database và integration](details/database-integrations.md)
- [Testing](details/testing.md)

## Đã triển khai hiện tại

API map notification/batch endpoints; Worker đăng ký snapshot và dispatch consumer; API/Worker cấu hình Entity Framework Outbox. Xem [API docs](../../../api/notification-service/README.md).

## Định hướng/chưa triển khai

Chỉ coi retry/lease/chunk là runtime khi handler source xác nhận. Scheduler không sở hữu notification content hoặc recipient state.
