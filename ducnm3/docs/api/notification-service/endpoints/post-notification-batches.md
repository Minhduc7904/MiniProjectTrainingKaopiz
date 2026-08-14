# `POST /api/notification-batches`

## Mục đích

Tạo lô gửi thông báo và chụp danh sách học viên đang hoạt động tại thời điểm nhận yêu cầu. API chỉ tạo bản chụp, lưu lô và phát lệnh dispatch; Notification Worker mới gửi inbox theo từng chunk.

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
    "totalCount": 1250,
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

- `202`: lô, snapshot người nhận và lệnh dispatch đã được chấp nhận.
- `400 VALIDATION_FAILED`: body không hợp lệ, scope khác `ALL_STUDENTS`, có `courseId`, hoặc snapshot rỗng.
- `503 STUDENT_SERVICE_UNAVAILABLE`: không thể lấy snapshot từ Student Service.
- `500 UNEXPECTED_ERROR`: phản hồi an toàn cho lỗi không mong đợi.

## Điều kiện nghiệp vụ và tác động phụ

API gọi Student Service theo trang (tối đa 100 học viên/trang), tạo `notification_batches` và `notification_batch_items`, rồi phát `DispatchNotificationBatchV1(batchId)`. HTTP request không gọi sender và không tạo `notifications`.

Notification Worker nhận command, mỗi lượt chỉ claim tối đa `batchSize` item `PENDING` hoặc `RETRY`, rồi tạo inbox `source_type=BULK`, `status=UNREAD`. Fake sender thử đúng một lần lại: thất bại lần đầu chuyển `RETRY`; thất bại lần hai chuyển `FAILED` và lưu lỗi. Nếu còn item, Worker tự gửi lại cùng command; không dùng Scheduler hay `background_jobs`.
