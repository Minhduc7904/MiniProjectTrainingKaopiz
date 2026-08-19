# `GET /notification/api/notification-batches/{batchId}/snapshot-status`

Business flow: [`get-notification-batch-snapshot-status.md`](../../../business-flows/notifications/get-notification-batch-snapshot-status.md).

## Mục đích

Notification Service trả trạng thái riêng của bước snapshot recipient để UI polling trước khi theo dõi delivery. Direct service path là `GET /api/notification-batches/{batchId}/snapshot-status`.

## Xác thực và phân quyền

MVP chưa có auth. Khi bổ sung auth, caller phải có quyền xem Notification Batch của tổ chức.

## Yêu cầu

`batchId` là UUID bắt buộc, khác rỗng. Không có query hoặc request body.

## Phản hồi thành công

`200 OK`, `Cache-Control: no-store`.

```json
{
  "data": {
    "batchId": "4c40bcf9-675e-435c-93bd-17cde82d1670",
    "status": "RUNNING",
    "snapshotCount": 1200,
    "targetCount": 3000,
    "progressPercent": 40
  },
  "meta": { "traceId": "01J..." }
}
```

`status` là `PENDING`, `RUNNING`, `COMPLETED` hoặc `FAILED`. `targetCount` và `progressPercent` có thể null khi request chọn toàn bộ và Student Service chưa chốt tổng recipient.

## Mã trạng thái HTTP

- `200`: batch tồn tại.
- `400 VALIDATION_FAILED`: `batchId` không hợp lệ.
- `404 NOTIFICATION_BATCH_NOT_FOUND`: batch không tồn tại.
- `500 UNEXPECTED_ERROR`: lỗi không mong đợi.

Lỗi dùng [error envelope chuẩn](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

Endpoint safe, idempotent, chỉ đọc `notification_batches` và đếm snapshot hiện có trong `notification_batch_items`; không gọi Student Service, không publish message và không thay đổi database.
