# Scheduled Unused Media Cleanup

## Mục đích

Mô tả boundary cho job dọn media không còn được sử dụng. Đây là thiết kế follow-up; foundation hiện chỉ có Scheduler schema, health API và Worker skeleton nên chưa tự xóa media.

## Actor

Scheduler Service; Scheduler Worker; Media Service.

## Điều kiện đầu vào

- Một `background_jobs` type `MEDIA_UNUSED_CLEANUP` sẽ được cấu hình `CRON`.
- Payload chỉ chứa tham số nhỏ như retention window; không chứa object key hoặc bản sao media metadata.
- Media Service là nơi duy nhất được query `media_usages`, quyết định object đủ điều kiện và gọi MinIO.

## Luồng dự kiến

1. Scheduler tính `next_run_at` từ CRON theo UTC.
2. Scheduler tạo `background_job_runs` với trigger `CRON` và idempotency key.
3. Worker claim run, đặt trạng thái `RUNNING` và gọi internal Media Service cleanup contract.
4. Media Service tìm `media_objects` không còn active usage và quá retention window.
5. Media Service xóa object qua `IStorage`, cập nhật metadata rồi trả summary an toàn.
6. Scheduler lưu summary vào `output_json` và kết thúc `SUCCEEDED`; lỗi được lưu bằng code/message an toàn.

## Retry và an toàn

- Unique `(background_job_id, idempotency_key)` chặn tạo trùng run.
- Media Service handler phải idempotent; object đã xóa không được coi là lỗi fatal.
- Scheduler không nhận MinIO credential và không query `lms_media_db`.
- Concurrency, claim lock, CRON parsing và internal HTTP contract chưa được triển khai trong phase hiện tại.

## Dữ liệu thay đổi

- Scheduler database: `background_job_runs` trong phase execution tương lai.
- Media database và MinIO: chỉ Media Service được thay đổi.
- Notification database: không thay đổi.
