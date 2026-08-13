# Dọn dẹp media không sử dụng theo lịch

## Mục đích

Mô tả ranh giới của tác vụ dọn media không còn được sử dụng và phục hồi các
upload `PENDING` stale. Đây là thiết kế cho giai đoạn sau; Scheduler hiện chưa
tự xử lý hai loại dữ liệu này.

## Tác nhân

Scheduler Service; Scheduler Worker; Media Service.

## Điều kiện đầu vào

- Một `background_jobs` loại `MEDIA_UNUSED_CLEANUP` sẽ được cấu hình `CRON`.
- Dữ liệu đầu vào chỉ chứa tham số nhỏ như thời gian lưu giữ; không chứa khóa đối tượng hoặc bản sao siêu dữ liệu media.
- Media Service là nơi duy nhất được truy vấn `media_usages`, quyết định đối tượng đủ điều kiện và gọi MinIO.
- Một job riêng hoặc một pha của cleanup sẽ dùng ngưỡng thời gian để nhận diện
  `media_objects.status = PENDING` stale; Scheduler không tự sửa database Media.

## Luồng dự kiến

1. Scheduler tính `next_run_at` từ CRON theo UTC.
2. Scheduler tạo `background_job_runs` với kiểu kích hoạt `CRON` và khóa lũy đẳng.
3. Worker nhận lượt chạy để xử lý, đặt trạng thái `RUNNING` và gọi hợp đồng dọn dẹp nội bộ của Media Service.
4. Media Service tìm `media_objects` không còn lượt sử dụng hoạt động và đã quá thời gian lưu giữ.
5. Media Service xóa đối tượng qua `IStorage`, cập nhật siêu dữ liệu rồi trả về bản tóm tắt an toàn.
6. Scheduler lưu bản tóm tắt vào `output_json` và kết thúc với `SUCCEEDED`; lỗi được lưu bằng mã/thông báo an toàn.

Với upload `PENDING` stale, Media Service phải kiểm tra database và MinIO theo
hợp đồng cleanup idempotent, xóa object mồ côi nếu có rồi chuyển bản ghi sang
`FAILED` hoặc trạng thái kết thúc được thiết kế sau. Luồng upload đồng bộ hiện đã
có compensation `PENDING -> FAILED`; job chỉ xử lý khoảng trống khi process dừng
trước khi compensation hoàn tất.

## Thử lại và an toàn

- Ràng buộc duy nhất `(background_job_id, idempotency_key)` chặn tạo trùng lượt chạy.
- Bộ xử lý của Media Service phải có tính lũy đẳng; đối tượng đã xóa không được coi là lỗi nghiêm trọng.
- Scheduler không nhận thông tin xác thực MinIO và không truy vấn `lms_media_db`.
- Xử lý đồng thời, khóa nhận xử lý, phân tích CRON, hợp đồng HTTP nội bộ và quét
  `PENDING` stale chưa được triển khai trong giai đoạn hiện tại.

## Dữ liệu thay đổi

- Cơ sở dữ liệu Scheduler: `background_job_runs` trong giai đoạn thực thi tương lai.
- Cơ sở dữ liệu Media và MinIO: chỉ Media Service được thay đổi.
- Cơ sở dữ liệu Notification Service: không thay đổi.
