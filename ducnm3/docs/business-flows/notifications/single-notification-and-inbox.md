# Single Notification and Student Inbox

## Mục đích

Admin gửi một notification cho một Student; Student chỉ xem và đánh dấu đã đọc notification của chính mình.

## Actor

Admin; Student; Notification Service.

## Điều kiện đầu vào

- Admin đã được xác thực và được phép gửi notification.
- Recipient Student tồn tại và có trạng thái `ACTIVE`.
- `bodyMarkdown` là Markdown hợp lệ và đã có media usage nếu nội dung nhúng media.

## Luồng gửi đơn

1. Admin gửi `POST /api/notifications` với `studentId`, `title`, và `bodyMarkdown`.
2. Notification Service validate dữ liệu và xác nhận Student.
3. Notification Service tạo một record `notifications`:
   - `recipient_student_id` là Student nhận.
   - `source_type` là `SINGLE`.
   - `status` là `UNREAD`.
   - `notification_job_id` là `null`.
4. API trả inbox item đã tạo.

## Luồng đọc inbox

1. Student gọi `GET /api/notifications/me`.
2. Notification Service lọc bằng identity đang xác thực, không nhận `studentId` từ client.
3. Student chọn một item và gọi `PATCH /api/notifications/{id}/read`.
4. Notification Service kiểm tra `recipient_student_id` trùng Student hiện tại.
5. Service chuyển `status` sang `READ` và lưu `read_at`.

## Trường hợp lỗi

- `403`: Admin không có quyền gửi hoặc Student cố đọc notification của người khác.
- `404`: Student hoặc notification không tồn tại.
- `409`: notification đã ở trạng thái `READ` nếu API chọn xử lý conflict thay vì idempotent success.

## Dữ liệu thay đổi

- Notification Service database: một record `notifications`.
- Không tạo `notification_jobs` hoặc `notification_job_items`.
