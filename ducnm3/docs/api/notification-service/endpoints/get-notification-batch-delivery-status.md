# `GET /notification/api/notification-batches/{batchId}/delivery-status`

Business flow: [`get-notification-batch-delivery-status.md`](../../../business-flows/notifications/get-notification-batch-delivery-status.md).

## Mục đích

Notification Service trả counter và thời gian của riêng bước gửi notification. Direct service path là `GET /api/notification-batches/{batchId}/delivery-status`.

## Xác thực và phân quyền

MVP chưa có auth. Quyền xem sẽ theo chính sách Notification Batch của tổ chức.

## Yêu cầu

`batchId` là UUID bắt buộc. Không có query hoặc request body.

## Phản hồi thành công

`200 OK`, `Cache-Control: no-store`.

```json
{
  "data": {
    "batchId": "4c40bcf9-675e-435c-93bd-17cde82d1670",
    "status": "RUNNING",
    "totalCount": 3000,
    "processedCount": 1500,
    "successCount": 1490,
    "failedCount": 10,
    "remainingCount": 1500,
    "progressPercent": 50,
    "startedAtUtc": "2026-08-18T01:00:02Z",
    "completedAtUtc": null,
    "durationMs": 12500
  },
  "meta": { "traceId": "01J..." }
}
```

Trước delivery, status là `PENDING`; khi gửi là `RUNNING`; thành công hoàn toàn là `COMPLETED`; batch `PARTIAL_FAILED` hoặc `FAILED` được biểu diễn `FAILED` ở cấp step nhưng counter vẫn phân biệt thành công/thất bại.

## Mã trạng thái HTTP

- `200`: batch tồn tại.
- `400 VALIDATION_FAILED`: UUID sai.
- `404 NOTIFICATION_BATCH_NOT_FOUND`: batch không tồn tại.
- `500 UNEXPECTED_ERROR`: lỗi không mong đợi.

## Điều kiện nghiệp vụ và tác động phụ

Endpoint chỉ đọc `notification_batches`, không claim recipient, không gửi notification và không phát command. UI gọi endpoint này sau khi snapshot `COMPLETED`.
