# `POST /api/notification-batches`

## Mục đích

Tạo một lô gửi hàng loạt thuộc sở hữu của Notification Service và lưu bản chụp danh sách người nhận. Việc thực thi chưa được triển khai trong giai đoạn nền tảng hiện tại.

## Xác thực và phân quyền

- Xác thực: bắt buộc.
- Vai trò/phạm vi: quản trị viên có quyền phát thông báo.
- Quy tắc sở hữu: Notification Service ghi nhận quản trị viên đã xác thực vào `created_by`.

## Yêu cầu

Không có tham số đường dẫn hoặc truy vấn.

```json
{
  "title": "Course update",
  "bodyMarkdown": "New material is available.",
  "targetScope": "COURSE_ENROLLED",
  "courseId": "2e71fdd3-a599-46d5-93e8-041e3b25b2b2",
  "batchSize": 500
}
```

- `title`: chuỗi bắt buộc, tối đa 200 ký tự.
- `bodyMarkdown`: Markdown an toàn, bắt buộc.
- `targetScope`: `COURSE_ENROLLED`, `STUDENT_IDS` hoặc `ALL_STUDENTS`.
- `courseId`: UUID chỉ bắt buộc với `COURSE_ENROLLED`.
- `batchSize`: số nguyên từ 100 đến 1000.

## Phản hồi thành công

```http
202 Accepted
```

```json
{
  "data": {
    "id": "4c40bcf9-675e-435c-93bd-17cde82d1670",
    "status": "PENDING",
    "totalCount": 0
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Mã trạng thái HTTP

- `202`: lô và bản chụp người nhận đã được chấp nhận.
- `400 VALIDATION_FAILED`: nội dung yêu cầu hoặc phạm vi đích không hợp lệ.
- `401`: thiếu thông tin xác thực hoặc thông tin xác thực không hợp lệ.
- `403`: bên gọi không có quyền phát thông báo.
- `404 COURSE_NOT_FOUND`: khóa học được yêu cầu không tồn tại.
- `409 BATCH_CONFLICT`: một lô tương đương đang hoạt động xung đột với yêu cầu.
- `500 UNEXPECTED_ERROR`: phản hồi an toàn cho lỗi không mong đợi.

## Điều kiện nghiệp vụ và tác động phụ

Tạo `notification_batches` và `notification_batch_items`. Hiện chưa tạo tác vụ Scheduler; tích hợp `NOTIFICATION_BATCH_DISPATCH` trong tương lai sẽ chỉ truyền `batchId` logic. Cơ chế thử lại và việc thực thi tiến trình xử lý vẫn là công việc tiếp theo.
