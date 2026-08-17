# Media Service — Worker

## Mục đích

Worker nhận message nền cho thumbnail và notification-media usage.

## Kiến trúc

```mermaid
flowchart LR
  MQ[RabbitMQ] --> C[Thumbnail / media usage consumers]
  C --> App[Application]
  App --> DB[(Media DB)]
  App --> MinIO
```

## Cách dùng

Worker đăng ký consumer qua shared Messaging và dùng cùng Application/Infrastructure.

## Đã triển khai hiện tại

Có generate-thumbnail consumer, fault consumer và notification media usage consumer.

## Định hướng/chưa triển khai

Không có Scheduler-driven stale-PENDING cleanup runtime.

## Troubleshooting

| Hiện tượng | Cách xử lý |
| --- | --- |
| Consumer không chạy | Kiểm tra RabbitMQ config và consumer registration. |
