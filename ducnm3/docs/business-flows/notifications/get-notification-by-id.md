# Xem chi tiết thông báo

## Mục đích

Đọc một notification theo ID để theo dõi kết quả gửi đơn hoặc item thành công của batch.

## Tác nhân và điều kiện

Client gọi Gateway với UUID `notificationId`. MVP chưa áp dụng auth.

## UML luồng chạy

```mermaid
sequenceDiagram
    participant Client
    participant Gateway
    participant API as Notification API
    participant DB as MySQL Notification

    Client->>Gateway: GET /notification/api/notifications/{notificationId}
    Gateway->>API: GET /api/notifications/{notificationId}
    API->>API: Validate UUID
    alt UUID invalid
        API-->>Client: 400 VALIDATION_FAILED
    else UUID valid
        API->>DB: SELECT notification by id
        alt Not found
            DB-->>API: none
            API-->>Client: 404 NOTIFICATION_NOT_FOUND
        else Found
            DB-->>API: notification detail
            API-->>Client: 200 + Cache-Control: no-store
        end
    end
```

## Dữ liệu thay đổi và kiểm thử

Endpoint chỉ đọc `notifications`; không tạo media usage hoặc thay đổi batch/status. Contract: [`GET notification by id`](../../api/notification-service/endpoints/get-notification-by-id.md).
