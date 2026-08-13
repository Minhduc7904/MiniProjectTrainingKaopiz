# Thông báo hàng loạt

## Mục đích

Quản trị viên tạo một lô nội dung và bản chụp danh sách người nhận trong Notification Service. Việc Scheduler gọi bộ xử lý và Worker phân phối xử lý người nhận thuộc giai đoạn sau, chưa được triển khai trong phần nền tảng hiện tại.

## Tác nhân

Quản trị viên; API của Notification Service; Student Service. Scheduler Service và Scheduler Worker chỉ là tác nhân trong tương lai.

## Điều kiện đầu vào

- Quản trị viên đã được xác thực và có quyền phát thông báo hàng loạt.
- `targetScope` hợp lệ: `COURSE_ENROLLED`, `STUDENT_IDS`, hoặc `ALL_STUDENTS`.
- Nội dung Markdown và các lượt sử dụng media đã được chuẩn bị.

## Luồng chính

1. Quản trị viên gửi `POST /api/notification-batches` với nội dung và phạm vi đối tượng.
2. Notification Service tạo `notification_batches` trạng thái `PENDING`.
3. API chụp lại danh sách người nhận và tạo `notification_batch_items`.
4. API trả `202 Accepted` cùng `batchId`.
5. Phần nền tảng dừng tại đây; chưa tự tạo `background_jobs` hoặc `background_job_runs`.

Luồng dự kiến trong giai đoạn thực thi:

1. Scheduler tạo lượt chạy của loại tác vụ `NOTIFICATION_BATCH_DISPATCH` với dữ liệu đầu vào nhỏ chỉ chứa `batchId`.
2. Scheduler Worker nhận lượt chạy để xử lý và gọi hợp đồng nội bộ của Notification Service.
3. Bộ xử lý của Notification Service lấy các mục `PENDING` hoặc `RETRY` theo `batch_size`.
4. Bộ xử lý tạo `notifications`, cập nhật mục/các bộ đếm và kết thúc lô.
5. Scheduler chỉ lưu trạng thái/kết quả tổng quát của lượt chạy, không sao chép nội dung hoặc danh sách người nhận.

## Thử lại và tính lũy đẳng

1. Nếu một mục thất bại, bộ xử lý của Notification Service trong tương lai tăng `retry_count` và chuyển sang `RETRY`.
2. Sau số lần thử lại tối đa, bộ xử lý lưu `error_message` và chuyển mục sang `FAILED`.
3. `UNIQUE(batch_id, student_id)` ngăn chụp trùng Học viên.
4. `UNIQUE(notification_batch_id, recipient_student_id)` ngăn tạo trùng mục hộp thư đến.
5. `UNIQUE(background_job_id, idempotency_key)` bảo đảm tính lũy đẳng cho lượt chạy Scheduler, không thay thế hai ràng buộc nghiệp vụ trên.

## Trường hợp lỗi

- `400`: phạm vi đối tượng hoặc Markdown không hợp lệ.
- `404`: Khóa học hoặc danh sách Học viên không tồn tại.
- `409`: lô xung đột trạng thái hoặc yêu cầu tương đương đang hoạt động.
- `202`: yêu cầu hợp lệ; việc xử lý chưa hoàn tất, không phải lỗi.

## Dữ liệu thay đổi

- Cơ sở dữ liệu Notification Service: `notification_batches`, `notification_batch_items`, `notifications`.
- Cơ sở dữ liệu Scheduler Service trong tương lai chỉ thay đổi `background_job_runs`.
- Student Service chỉ được đọc để lấy danh sách người nhận.
