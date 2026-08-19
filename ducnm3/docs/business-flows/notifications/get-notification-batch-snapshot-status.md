# Lấy trạng thái snapshot Notification Batch

## Mục đích

Cho UI biết Notification Worker đã snapshot bao nhiêu recipient trước khi bắt đầu gọi delivery status.

## Tác nhân

Admin UI, Gateway, Notification API và MySQL Notification.

## Luồng chính

1. UI gọi `GET /notification/api/notification-batches/{batchId}/snapshot-status`.
2. Notification API validate UUID và đọc batch cùng số `notification_batch_items`.
3. Application map trạng thái batch thành `PENDING`, `RUNNING`, `COMPLETED` hoặc `FAILED`.
4. API trả `200`, `no-store` và progress khi có mẫu số.
5. UI chỉ chuyển sang delivery status khi snapshot là `COMPLETED`.

## Trường hợp lỗi

- UUID sai trả `400 VALIDATION_FAILED`.
- Batch không tồn tại trả `404 NOTIFICATION_BATCH_NOT_FOUND`.

## Dữ liệu thay đổi

Không có; endpoint chỉ đọc Notification DB.
