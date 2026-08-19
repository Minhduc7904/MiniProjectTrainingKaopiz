# `POST /api/notification-batches/{batchId}/retry-failed`

## Mục đích

Tạo batch con chỉ gồm recipient có item `FAILED` trong batch nguồn; không mở lại nguồn và không gửi lại item `SUCCESS`.

## Yêu cầu

```json
{ "createdBy": "2e71fdd3-a599-46d5-93e8-041e3b25b2b2" }
```

Batch nguồn phải terminal và có `failedCount > 0`.

## Phản hồi

- `202 Accepted` cùng `Location` của batch con.
- `404 NOTIFICATION_BATCH_NOT_FOUND` nếu nguồn không tồn tại.
- `409 NOTIFICATION_BATCH_NOT_TERMINAL` nếu nguồn còn chạy.
- `409 NOTIFICATION_BATCH_HAS_NO_FAILED_ITEMS` nếu không có lỗi.

`source_batch_id` là unique: gọi lại trực tiếp trên cùng nguồn trả cùng batch con. Batch retry có thể tiếp tục được retry, tạo thành chuỗi. Snapshot dùng `INSERT ... SELECT` chỉ lấy item `FAILED`, không gọi Student Service.
