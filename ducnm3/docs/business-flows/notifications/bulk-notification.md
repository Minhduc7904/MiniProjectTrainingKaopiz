# Bulk Notification

## Mục đích

Admin tạo một batch nội dung và recipient snapshot trong Notification Service. Việc Scheduler gọi handler và delivery worker xử lý recipient là phase sau, chưa được triển khai trong foundation hiện tại.

## Actor

Admin; Notification Service API; Student Service. Scheduler Service và Scheduler Worker chỉ là actor tương lai.

## Điều kiện đầu vào

- Admin đã được xác thực và có quyền broadcast.
- `targetScope` hợp lệ: `COURSE_ENROLLED`, `STUDENT_IDS`, hoặc `ALL_STUDENTS`.
- Nội dung Markdown và media usages đã được chuẩn bị.

## Luồng chính

1. Admin gửi `POST /api/notification-batches` với nội dung và target scope.
2. Notification Service tạo `notification_batches` trạng thái `PENDING`.
3. API snapshot recipient list và tạo `notification_batch_items`.
4. API trả `202 Accepted` cùng `batchId`.
5. Foundation dừng tại đây; chưa tự tạo `background_jobs` hoặc `background_job_runs`.

Luồng dự kiến ở phase execution:

1. Scheduler tạo run của job type `NOTIFICATION_BATCH_DISPATCH` với payload nhỏ chỉ chứa `batchId`.
2. Scheduler Worker claim run và gọi internal contract của Notification Service.
3. Notification handler lấy item `PENDING` hoặc `RETRY` theo `batch_size`.
4. Handler tạo `notifications`, cập nhật item/counters và kết thúc batch.
5. Scheduler chỉ lưu run status/output tổng quát, không sao chép content hoặc recipient list.

## Retry và idempotency

1. Nếu một item thất bại, Notification handler tương lai tăng `retry_count` và chuyển sang `RETRY`.
2. Sau số retry tối đa, handler lưu `error_message` và chuyển item sang `FAILED`.
3. `UNIQUE(batch_id, student_id)` ngăn snapshot trùng Student.
4. `UNIQUE(notification_batch_id, recipient_student_id)` ngăn tạo inbox item trùng.
5. `UNIQUE(background_job_id, idempotency_key)` là idempotency của Scheduler run, không thay thế hai constraint nghiệp vụ trên.

## Trường hợp lỗi

- `400`: target scope hoặc Markdown không hợp lệ.
- `404`: Course hoặc Student list không tồn tại.
- `409`: batch xung đột trạng thái hoặc request tương đương đang active.
- `202`: request hợp lệ; việc xử lý chưa hoàn tất, không phải lỗi.

## Dữ liệu thay đổi

- Notification Service database: `notification_batches`, `notification_batch_items`, `notifications`.
- Scheduler Service database tương lai chỉ thay đổi `background_job_runs`.
- Student Service chỉ được đọc để lấy recipient.
