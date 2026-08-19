# Kiểm thử component Notification Service

## Phạm vi

Dự án: `backend/Services/Notification/NotificationService.ComponentTests`.

TestServer khởi động HTTP pipeline thật với repository/message sender in-memory. Test
không kết nối MySQL, RabbitMQ hoặc service ngoài.

Chạy:

```bash
dotnet test backend/Services/Notification/NotificationService.ComponentTests/NotificationService.ComponentTests.csproj
```

## Ca kiểm thử

| Nhóm endpoint | Đạt khi |
| --- | --- |
| `POST /api/notifications` và `GET /api/notifications/{id}` | POST trả `201` cùng public `Location`; GET trả `200`, `UNREAD` và `Cache-Control: no-store`; UUID sai trả `400`. |
| `POST /api/notification-batches` và `GET /api/notification-batches/{id}` | POST trả `202` cùng public `Location`; GET trả summary `PENDING`; target scope chưa hỗ trợ trả `400`. |
| `GET /api/notification-batches/{id}/failed-items` | Trả envelope cursor, `items` và `Cache-Control: no-store` đúng contract. |
| Snapshot/delivery status | Hai route status trả `200`, `no-store`, snapshot count và delivery remaining qua TestServer. |

Mỗi route được map bởi một endpoint file riêng. Component tests khóa lại HTTP status,
header và envelope trong khi unit tests tập trung Domain/Application behavior.
