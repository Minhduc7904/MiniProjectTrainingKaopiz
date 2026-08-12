# Bulk Notification

## Mục đích

Admin gửi cùng một notification cho nhiều Student mà không giữ HTTP request mở trong thời gian worker xử lý.

## Actor

Admin; Notification Service API; Notification Worker; Student Service.

## Điều kiện đầu vào

- Admin đã được xác thực và có quyền broadcast.
- `targetScope` hợp lệ: `COURSE_ENROLLED`, `STUDENT_IDS`, hoặc `ALL_STUDENTS`.
- Nội dung Markdown và media usages đã được chuẩn bị.

## Luồng chính

1. Admin gửi `POST /api/notification-jobs` với nội dung và target scope.
2. Notification Service tạo `notification_jobs` trạng thái `PENDING`.
3. API snapshot recipient list từ Student Service hoặc enrollment, tạo `notification_job_items`.
4. API trả `202 Accepted` cùng `jobId`.
5. Background worker lấy các item `PENDING` hoặc `RETRY` theo chunk `batch_size`.
6. Với mỗi Student, worker tạo một `notifications` item có `source_type = BULK`.
7. Worker lưu `notification_id`, cập nhật job item `SUCCESS`, và tăng counters của job.
8. Khi không còn item, worker cập nhật job thành `COMPLETED` hoặc `PARTIAL_FAILED`.

## Retry và idempotency

1. Nếu một item thất bại, worker tăng `retry_count` và chuyển sang `RETRY`.
2. Sau số retry tối đa, worker lưu `error_message` và chuyển item sang `FAILED`.
3. `UNIQUE(job_id, student_id)` ngăn tạo nhiều job item cho một Student.
4. `UNIQUE(notification_job_id, recipient_student_id)` ngăn worker tạo notification trùng khi restart hoặc retry.

## Trường hợp lỗi

- `400`: target scope hoặc Markdown không hợp lệ.
- `404`: Course hoặc Student list không tồn tại.
- `409`: job không còn có thể chạy lại theo trạng thái hiện tại.
- `202`: request hợp lệ; việc xử lý chưa hoàn tất, không phải lỗi.

## Dữ liệu thay đổi

- Notification Service database: `notification_jobs`, `notification_job_items`, `notifications`.
- Student Service chỉ được đọc để lấy recipient.
