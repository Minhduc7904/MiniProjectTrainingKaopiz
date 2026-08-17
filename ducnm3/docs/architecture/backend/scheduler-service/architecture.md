# Kiến trúc Scheduler Service

## Mục đích và ownership

Scheduler sở hữu background jobs và background job runs. Xem [data model](../../../database/scheduler-service/data-model.md). Nó không sở hữu notification, recipient, media hoặc database service khác.

~~~mermaid
flowchart LR
  Gateway --> Api[Scheduler API] --> App[Application] --> Domain
  App --> Infra[Infrastructure] --> Db[(Scheduler database)]
  MQ[RabbitMQ] --> Worker[Scheduler Worker host]
~~~

## Các tầng

- **Domain:** model BackgroundJob/BackgroundJobRun.
- **Application:** boundary use case/port cho scheduler.
- **Infrastructure:** persistence, migration và health probe.
- **API/Worker:** API health/info; Worker host MassTransit.

## Tài liệu chi tiết

- [Domain](details/domain.md)
- [Application](details/application.md)
- [Infrastructure](details/infrastructure.md)
- [API](details/api.md)
- [Worker](details/worker.md)
- [Database và integration](details/database-integrations.md)
- [Testing](details/testing.md)

## Đã triển khai hiện tại

Schema, DbContext, migration, API health/info và Worker host đã có. [API docs](../../../api/scheduler-service/README.md) hiện chỉ ghi health contract.

## Định hướng/chưa triển khai

Cron parsing, tính next run, polling, lease/claim, job handler, retry execution và gọi service đích chưa là runtime behavior.
