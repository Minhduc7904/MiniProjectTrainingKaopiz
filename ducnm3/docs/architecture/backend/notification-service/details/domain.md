# Notification Service — Domain

## Mục đích

Domain giữ notification, batch, item status và target scope độc lập transport.

## Kiến trúc

```mermaid
flowchart LR
  Application --> Domain
  Domain -.-> Tech[EF Core / HTTP / RabbitMQ]
```

## Cách dùng

Handler Application áp dụng status/rule; recipient/content adapter nằm ngoài Domain.

## Đã triển khai hiện tại

`NotificationTypes` khai báo source/status/batch/item/target scope constants.

## Định hướng/chưa triển khai

Không mang Student/Media model hoặc delivery provider SDK vào Domain.

## Troubleshooting

| Hiện tượng | Cách xử lý |
| --- | --- |
| Status bị sai layer | Đặt transition rule Domain/Application, không ở endpoint. |
