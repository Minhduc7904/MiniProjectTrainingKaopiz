# `GET /media/api/media/usage-jobs/{jobId}/status`

Business flow: [`get-notification-media-usage-job-status.md`](../../../business-flows/notifications/get-notification-media-usage-job-status.md).

## Mục đích

Media Service trả tiến độ background job đăng ký Media Usage từ Markdown của Notification Batch. Direct service path là `GET /api/media/usage-jobs/{jobId}/status`; `jobId` dùng cùng UUID với Notification Batch.

## Xác thực và phân quyền

MVP chưa có auth. Sau khi có auth, caller phải có quyền xem source Notification Batch tương ứng.

## Yêu cầu

`jobId` là UUID bắt buộc. Không có query hoặc request body.

## Phản hồi thành công

`200 OK`, `Cache-Control: no-store`.

```json
{
  "data": {
    "jobId": "4c40bcf9-675e-435c-93bd-17cde82d1670",
    "status": "PROCESSING",
    "expectedUsageCount": 3000,
    "processedUsageCount": 1500,
    "failedUsageCount": 0,
    "remainingUsageCount": 1500,
    "progressPercent": 50,
    "createdAtUtc": "2026-08-18T01:00:01Z",
    "startedAtUtc": "2026-08-18T01:00:10Z",
    "completedAtUtc": null,
    "errorMessage": null
  },
  "meta": { "traceId": "01J..." }
}
```

Status gồm `PENDING`, `PROCESSING`, `COMPLETED`, `PARTIAL_FAILED`, `FAILED`. `expectedUsageCount` và progress null cho đến khi Notification Service gửi completion marker; Markdown không chứa media tạo job `COMPLETED` với expected bằng `0`.

## Mã trạng thái HTTP

- `200`: job tồn tại.
- `400 INVALID_MEDIA`: UUID sai.
- `404 NOTIFICATION_MEDIA_USAGE_JOB_NOT_FOUND`: start command chưa tới hoặc job không tồn tại.
- `500 UNEXPECTED_ERROR`: lỗi không mong đợi.

## Điều kiện nghiệp vụ và tác động phụ

Endpoint chỉ đọc `notification_media_usage_jobs`. Worker cập nhật counter nguyên tử; MassTransit Inbox chống xử lý lại cùng message. GET không tạo job hoặc Media Usage.
