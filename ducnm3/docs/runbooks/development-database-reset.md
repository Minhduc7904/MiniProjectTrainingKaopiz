# Sổ tay đặt lại cơ sở dữ liệu phát triển

Chỉ dùng sổ tay này cho cơ sở dữ liệu phát triển cục bộ có thể xóa bỏ khi đường
cơ sở `V001` sạch thay đổi và không được giữ lại checksum cũ trong `schema_migrations`.

## Điều kiện tiên quyết

- `.env` tồn tại và chứa `ASPNETCORE_ENVIRONMENT=Development`.
- Năm tên cơ sở dữ liệu chính xác là `lms_course_db`, `lms_student_db`,
  `lms_media_db`, `lms_notification_db` và `lms_scheduler_db`.
- Không có dữ liệu MySQL cục bộ nào cần được giữ lại.
- Dữ liệu MinIO phải được giữ nguyên.

## Quy trình

```bash
scripts/database/reset-development-databases.sh --confirm
docker compose up -d --build
```

Tập lệnh có cơ chế bảo vệ dừng các container API, khởi động/kiểm tra sức khỏe MySQL,
chỉ xóa năm cơ sở dữ liệu dự kiến, buộc tạo lại `mysql-init` và giữ nguyên
`minio-data`. Khi khởi động, API sẽ áp dụng `V001` sạch của từng dịch vụ.

Thao tác đặt lại này cũng xóa mọi bản ghi do `Lms.DataSeeder` tạo, vì dữ liệu
được ghi vào `lms_student_db` và `lms_course_db` thay vì một cơ sở dữ liệu hoặc
migration riêng. Không cần lệnh dọn dữ liệu bổ sung. Để tạo lại tập dữ liệu mặc
định sau khi các dịch vụ áp dụng migration:

```bash
scripts/seed/run-development-seed.sh --confirm
```

## Xác minh

Mỗi cơ sở dữ liệu phải chứa chính xác một bản ghi `schema_migrations` có phiên
bản `001`. Notification phải chứa `notification_batches`,
`notification_batch_items` và `notifications`; Scheduler chỉ được chứa
`background_jobs` và `background_job_runs` dưới dạng bảng nghiệp vụ.
`students`, `courses`, `lessons` và `enrollments` phải rỗng trước lần tạo dữ liệu mới.

## Lỗi và khôi phục

- Việc thiếu `--confirm`, môi trường không phải Development hoặc tên cơ sở dữ
  liệu ngoài dự kiến phải làm quy trình dừng trước khi xóa bất kỳ dữ liệu nào.
- Nếu thao tác đặt lại thất bại sau khi xóa cơ sở dữ liệu, hãy sửa MySQL/Compose
  rồi chạy lại cùng tập lệnh đã xác nhận. Không tự chèn/xóa bản ghi lịch sử migration.
- Nếu một câu lệnh DDL trong V001 thất bại, hãy sửa trước khi dùng chung, chạy
  lại toàn bộ thao tác đặt lại cục bộ và áp dụng lại tất cả migration. DDL của
  MySQL có thể tự động commit.
- Không có thao tác hoàn tác để khôi phục dữ liệu MySQL đã xóa. Hãy khôi phục từ
  bản sao lưu nếu giả định dữ liệu có thể xóa bỏ là sai.
