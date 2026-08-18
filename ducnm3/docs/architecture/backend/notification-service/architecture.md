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

- **Domain:** sở hữu transition của batch/item và các hằng số nghiệp vụ; không tham chiếu framework.
- **Application:** tổ chức theo use case và chỉ phụ thuộc các port repository/client/sender.
- **Infrastructure:** hiện thực port bằng EF Core, persistence mapper, Student client, sender và Outbox.
- **API:** mỗi route nằm trong một endpoint file riêng; contract và response mapper không lẫn persistence.
- **Worker:** consumer chỉ chuyển message vào Snapshot/Dispatch handler.

## Cấu trúc source

```text
NotificationService.Domain/
  Constants/                 # Status, source type, target scope
  Entities/                  # Batch/item state và transition
NotificationService.Application/
  Common/Errors/
  Contracts/Messaging/
  Repositories/              # Port và model qua boundary persistence
  Services/                  # Port content, sender, Student client
  UseCases/
    Notifications/{Create,GetById}/
    NotificationBatches/{Create,GetById,GetFailedItems,Snapshot,Dispatch}/
NotificationService.Infrastructure/
  Clients/Student/
  Persistence/{Context,Mappers,Repositories,Scaffolded}/
  Services/Sending/
NotificationService.Api/
  Contracts/
  Endpoints/<Resource>/<UseCase>/
  Mappers/
NotificationService.Worker/Consumers/NotificationBatches/
```

`Scaffolded` chỉ là EF database model. Mapper tại Infrastructure chuyển dữ liệu
giữa scaffolded entity, Domain state và Application model; endpoint không truy cập
DbContext hoặc EF entity.

## Tài liệu chi tiết

- [Domain](details/domain.md)
- [Application](details/application.md)
- [Infrastructure](details/infrastructure.md)
- [API](details/api.md)
- [Worker](details/worker.md)
- [Database và integration](details/database-integrations.md)
- [Testing](details/testing.md)

## Đã triển khai hiện tại

API map năm route đã triển khai bằng năm endpoint riêng; Worker đăng ký snapshot và
dispatch consumer; API/Worker cấu hình Entity Framework Outbox. Batch repository đọc
và ghi qua persistence mapper, còn retry/status transition nằm trong Domain entity.
Xem [API docs](../../../api/notification-service/README.md).

## Định hướng/chưa triển khai

Chỉ coi retry/lease/chunk là runtime khi handler source xác nhận. Scheduler không sở hữu notification content hoặc recipient state.
