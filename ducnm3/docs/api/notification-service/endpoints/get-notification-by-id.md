# `GET /api/notifications/{notificationId}`

## Mục đích

Đọc chi tiết một notification theo ID. Public route là `GET /notification/api/notifications/{notificationId}`; API đặt `Cache-Control: no-store`.

## Phản hồi

Trả `200` với cùng `data` của `POST /api/notifications`, bao gồm `bodyMarkdown`, `sourceType`, `status` và các timestamp.

- `400 VALIDATION_FAILED`: `notificationId` không phải UUID.
- `404 NOTIFICATION_NOT_FOUND`: không tìm thấy notification.

MVP chưa có auth. Khi bổ sung auth, endpoint phải kiểm soát người gửi/người nhận được phép xem notification.
