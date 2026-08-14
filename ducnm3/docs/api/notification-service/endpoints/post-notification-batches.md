# `POST /api/notification-batches`

## Mục đích

Tạo operation gửi thông báo hàng loạt. API chỉ ghi batch `PENDING` và phát lệnh snapshot qua transactional outbox; Notification Worker chụp danh sách học viên đang hoạt động theo từng trang rồi mới dispatch inbox theo từng chunk. Vì vậy API không giữ request HTTP trong lúc lấy toàn bộ recipient.

## Xác thực và phân quyền

MVP hiện chưa có auth. Bên gọi gửi `createdBy` UUID để lưu người tạo lô. Khi có auth, API phải lấy giá trị này từ principal thay vì body.

## Yêu cầu

Không có tham số đường dẫn hoặc truy vấn.

```json
{
  "title": "Course update",
  "bodyMarkdown": "New material is available.",
  "targetScope": "ALL_STUDENTS",
  "createdBy": "2e71fdd3-a599-46d5-93e8-041e3b25b2b2",
  "batchSize": 500
}
```

- `title`: chuỗi bắt buộc, tối đa 200 ký tự.
- `bodyMarkdown`: Markdown bắt buộc.
- `targetScope`: MVP chỉ nhận chính xác `ALL_STUDENTS`.
- `createdBy`: UUID bắt buộc.
- `batchSize`: số nguyên từ 1 đến 1000; mặc định `500`.
- Không gửi `courseId`; các scope `COURSE_ENROLLED` và `STUDENT_IDS` chưa thuộc MVP.

## Phản hồi thành công

```http
202 Accepted
Location: /notification/api/notification-batches/4c40bcf9-675e-435c-93bd-17cde82d1670
```

```json
{
  "data": {
    "id": "4c40bcf9-675e-435c-93bd-17cde82d1670",
    "status": "PENDING",
    "totalCount": 0,
    "processedCount": 0,
    "successCount": 0,
    "failedCount": 0,
    "batchSize": 500,
    "createdAtUtc": "2026-08-14T01:00:00Z"
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

Poll `GET /api/notification-batches/{batchId}` qua gateway tại URL trong `Location` để xem trạng thái lô.

## Mã trạng thái HTTP

- `202`: batch và durable snapshot command đã được chấp nhận. `totalCount` là `0` cho tới khi Worker hoàn tất snapshot.
- `400 VALIDATION_FAILED`: body không hợp lệ, scope khác `ALL_STUDENTS`, có `courseId`, hoặc snapshot rỗng.
- `500 UNEXPECTED_ERROR`: phản hồi an toàn cho lỗi không mong đợi.

## Điều kiện nghiệp vụ và tác động phụ

API tạo một hàng `notification_batches` `PENDING` và ghi `SnapshotNotificationBatchV1(batchId)` vào MassTransit outbox trong cùng transaction. Worker commit trạng thái `SNAPSHOTTING` trước khi stream `GET /api/students?status=ACTIVE&pageSize=100`, rồi commit từng trang upsert vào `notification_batch_items` và chuyển batch sang `SNAPSHOT_READY`. Sau đó Worker phát `DispatchNotificationBatchV1(batchId)` trước khi ack command snapshot; nếu process dừng giữa hai thao tác, RabbitMQ redeliver command và Worker chỉ dispatch lại từ trạng thái `SNAPSHOT_READY`. HTTP request không gọi Student Service, sender hoặc tạo `notifications`.

Các trạng thái polling gồm `PENDING`, `SNAPSHOTTING`, `SNAPSHOT_READY`, `PROCESSING`, `COMPLETED`, `PARTIAL_FAILED` và `FAILED`. Nếu snapshot không lấy được Student Service hoặc không có recipient, Worker kết thúc batch ở `FAILED`; client tạo request mới khi dependency đã khôi phục.

Notification Worker nhận command, mỗi lượt chỉ claim tối đa `batchSize` item `PENDING` hoặc `RETRY`, rồi tạo inbox `source_type=BULK`, `status=UNREAD`. Fake sender thử đúng một lần lại: thất bại lần đầu chuyển `RETRY`; thất bại lần hai chuyển `FAILED` và lưu lỗi. Nếu còn item, Worker tự gửi lại cùng command; không dùng Scheduler hay `background_jobs`.
