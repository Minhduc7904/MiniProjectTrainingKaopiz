# LMS, Media, and Notification Data Model

Mỗi service sở hữu database riêng. Các ID tham chiếu sang service khác chỉ là logical reference; không tạo foreign key xuyên database.

## Course Service database

### courses

```text
id                    // UUID định danh Course
name                  // Tên Course hiển thị cho người dùng
description_markdown  // Nội dung mô tả Markdown; có thể nhúng media qua URL của Media Service
status                // DRAFT | PUBLISHED | ARCHIVED
created_at            // Thời điểm tạo Course, UTC
updated_at            // Thời điểm cập nhật gần nhất, UTC
```

### lessons

```text
id                    // UUID định danh Lesson
course_id             // UUID Course sở hữu Lesson, cùng Course Service database
title                 // Tiêu đề Lesson
display_order         // Thứ tự hiển thị Lesson trong Course
content_markdown      // Nội dung Markdown; có thể nhúng media qua URL của Media Service
created_at            // Thời điểm tạo Lesson, UTC
updated_at            // Thời điểm cập nhật gần nhất, UTC
```

### enrollments

```text
id                    // UUID định danh lượt ghi danh
course_id             // UUID Course được ghi danh
student_id            // UUID Student từ Student Service; logical reference, không có cross-DB FK
enrolled_at           // Thời điểm Student ghi danh, UTC
```

### lesson_progresses

```text
id                    // UUID định danh tiến độ học
lesson_id             // UUID Lesson được theo dõi tiến độ
student_id            // UUID Student từ Student Service; logical reference
progress_percent      // Phần trăm hoàn thành, 0–100
completed_at          // Thời điểm hoàn thành; nullable khi chưa hoàn thành
updated_at            // Thời điểm cập nhật tiến độ gần nhất, UTC
```

## Student Service database

### students

```text
id                    // UUID định danh Student
email                 // Email đăng nhập hoặc liên hệ; unique
display_name          // Tên hiển thị của Student
status                // ACTIVE | INACTIVE | BLOCKED
created_at            // Thời điểm tạo Student, UTC
updated_at            // Thời điểm cập nhật Student gần nhất, UTC
```

Index và ràng buộc:

```sql
UNIQUE(email)
INDEX(status, created_at DESC)
```

## Media Service database

### media_objects

Lưu metadata cho object trong MinIO; không lưu binary trong MySQL.

```text
id                    // UUID định danh media
bucket                // Tên bucket MinIO chứa object
object_key            // Khóa object duy nhất trong bucket; không trả trực tiếp cho client
media_type            // IMAGE | VIDEO | DOCUMENT | AUDIO | OTHER
content_type          // MIME type đã xác thực, ví dụ image/webp hoặc application/pdf
original_file_name    // Tên file do người dùng upload, chỉ để hiển thị
size_bytes            // Kích thước object theo byte
checksum_sha256       // Hash kiểm tra toàn vẹn và hỗ trợ phát hiện file trùng
uploaded_by           // UUID user/admin upload media
created_at            // Thời điểm upload hoàn tất, UTC
deleted_at            // Soft-delete timestamp; nullable khi media còn hoạt động
```

Index và ràng buộc:

```sql
UNIQUE(bucket, object_key)
INDEX(uploaded_by, created_at DESC)
```

### media_usages

Bảng này liên kết một media với vị trí sử dụng mà không để Course hoặc Notification Service sở hữu metadata media.

```text
id                    // UUID định danh liên kết usage
media_id              // UUID media_objects.id trong Media Service database
owner_service         // COURSE | NOTIFICATION; service sở hữu nội dung tham chiếu
owner_type            // COURSE_THUMBNAIL | COURSE_DESCRIPTION | LESSON_CONTENT | NOTIFICATION_BODY
owner_id              // UUID Course, Lesson, hoặc Notification ở owner_service; logical reference
usage_type            // THUMBNAIL | EMBED | ATTACHMENT
display_order         // Thứ tự render media trong cùng một owner
created_by            // UUID user/admin tạo liên kết
created_at            // Thời điểm tạo liên kết, UTC
deleted_at            // Soft-delete timestamp; nullable khi usage còn hiệu lực
```

Index và ràng buộc:

```sql
UNIQUE(media_id, owner_service, owner_type, owner_id, usage_type)
INDEX(owner_service, owner_type, owner_id, display_order)
```

Quy tắc `THUMBNAIL`:

- `COURSE_THUMBNAIL` là nguồn sự thật duy nhất cho thumbnail; bảng `courses` không lưu `thumbnail_media_id`.
- Media Service chỉ cho phép tối đa một `media_usages` active có `owner_type = COURSE_THUMBNAIL` cho mỗi `owner_id`.
- Khi thay thumbnail, Media Service soft-delete usage cũ và tạo usage mới trong cùng transaction.

## Notification Service database

Notification Service giữ nội dung và recipient snapshot của bulk delivery. Đây là dữ liệu nghiệp vụ, không phải metadata lịch chạy generic của Scheduler.

### notification_batches

```text
id                    // UUID định danh yêu cầu gửi hàng loạt
course_id             // UUID Course liên quan; nullable nếu target không theo Course
title                 // Tiêu đề notification dùng cho toàn batch
body_markdown         // Nội dung Markdown dùng cho toàn batch
target_scope          // COURSE_ENROLLED | STUDENT_IDS | ALL_STUDENTS
created_by            // UUID admin tạo batch; logical reference
status                // PENDING | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED
total_count           // Tổng recipient đã snapshot khi tạo batch
processed_count       // Số recipient đã được xử lý
success_count         // Số inbox item tạo thành công
failed_count          // Số recipient thất bại sau retry
batch_size            // Số recipient nghiệp vụ xử lý trên mỗi chunk
started_at            // Thời điểm bắt đầu xử lý, UTC; nullable khi chưa chạy
completed_at          // Thời điểm kết thúc, UTC; nullable khi chưa hoàn tất
created_at            // Thời điểm tạo batch, UTC
```

Status lifecycle dự kiến: `PENDING -> PROCESSING -> COMPLETED | PARTIAL_FAILED | FAILED`. Foundation hiện chỉ có schema; chưa có handler chuyển trạng thái.

### notification_batch_items

```text
id                    // UUID định danh recipient trong batch
batch_id              // UUID notification_batches.id trong cùng database
student_id            // UUID Student nhận notification; logical reference
notification_id       // UUID notifications.id được tạo; nullable khi chưa thành công
status                // PENDING | PROCESSING | SUCCESS | RETRY | FAILED
retry_count           // Số lần retry item nghiệp vụ đã thực hiện
error_message         // Lỗi cuối cùng; nullable khi chưa lỗi hoặc đã thành công
processed_at          // Thời điểm xử lý cuối, UTC; nullable khi chưa xử lý
```

`UNIQUE(batch_id, student_id)` chống snapshot trùng recipient. Xóa batch cascade các item; xóa inbox item chỉ đặt `notification_id` của item thành null.

### notifications

```text
id                    // UUID định danh inbox item
recipient_student_id  // UUID Student sở hữu notification; logical reference
title                 // Tiêu đề hiển thị trong inbox
body_markdown         // Nội dung Markdown; media nhúng dùng URL Media Service
source_type           // SINGLE | BULK
notification_batch_id // UUID notification_batches.id; nullable với SINGLE
created_by            // UUID admin hoặc system tạo notification; logical reference
status                // UNREAD | READ
read_at               // Thời điểm đánh dấu đã đọc, UTC; nullable khi UNREAD
created_at            // Thời điểm notification xuất hiện trong inbox, UTC
```

`UNIQUE(notification_batch_id, recipient_student_id)` chống tạo inbox item trùng khi retry. `notification_batch_id` dùng `ON DELETE RESTRICT` để không làm mất audit source của inbox BULK. Read lifecycle là `UNREAD -> READ`; `read_at` phải nhất quán với status.

## Scheduler Service database

Scheduler Service chỉ sở hữu định nghĩa job generic và lịch sử run. Nó không lưu notification content, recipient list, media metadata và không có foreign key tới database service khác.

### background_jobs

```text
id                    // UUID định danh cấu hình background job
job_key               // Khóa ổn định và duy nhất để đăng ký/tra cứu job
name                  // Tên hiển thị cho vận hành
job_type              // Loại handler tương lai, ví dụ NOTIFICATION_BATCH_DISPATCH
target_service        // Service nghiệp vụ tương lai sẽ được gọi; logical value
schedule_type         // MANUAL | CRON
cron_expression       // Biểu thức CRON theo UTC; bắt buộc với CRON, null với MANUAL
payload_json          // Cấu hình đầu vào nhỏ; không chứa recipient list/domain data lớn
status                // ACTIVE | PAUSED | DISABLED
allow_concurrent      // Cho phép nhiều run đồng thời của cùng job
max_retry_count       // Số retry tối đa cho execution phase tương lai
timeout_seconds       // Thời gian chạy tối đa; phải lớn hơn 0
next_run_at           // Thời điểm UTC chạy CRON kế tiếp; nullable khi chưa tính/MANUAL
created_by            // UUID actor tạo job; nullable với system-defined job
created_at            // Thời điểm tạo job, UTC
updated_at            // Thời điểm cập nhật job gần nhất, UTC
```

`UNIQUE(job_key)` chặn cấu hình trùng. Index `(status, next_run_at)` phục vụ due-job lookup tương lai. Foundation chưa parse CRON hoặc cập nhật `next_run_at`.

### background_job_runs

```text
id                    // UUID định danh một lần thực thi
background_job_id     // UUID background_jobs.id trong cùng Scheduler database
trigger_type          // MANUAL | CRON | RETRY
idempotency_key       // Khóa chống tạo trùng run cho cùng job
payload_snapshot_json // Snapshot payload tại thời điểm tạo run
attempt_number        // Lần thử hiện tại, bắt đầu từ 1
status                // QUEUED | RUNNING | SUCCEEDED | FAILED | CANCELLED | TIMED_OUT | SKIPPED
scheduled_at          // Thời điểm UTC run được lên lịch/manual trigger
started_at            // Thời điểm worker bắt đầu, UTC; nullable
finished_at           // Thời điểm worker kết thúc, UTC; nullable
worker_instance       // Định danh worker xử lý; nullable trước khi claim
correlation_id        // ID nối log Scheduler với target service
error_code            // Mã lỗi ổn định cuối cùng; nullable khi chưa lỗi
error_message         // Thông tin lỗi an toàn; không chứa credential
output_json           // Kết quả tóm tắt; không thay thế domain database
created_at            // Thời điểm tạo run, UTC
```

`UNIQUE(background_job_id, idempotency_key)` bảo đảm idempotency. Index `(status, scheduled_at)` phục vụ claim queue và `(background_job_id, created_at DESC)` phục vụ run history. FK chỉ nội bộ Scheduler và dùng `ON DELETE RESTRICT` để không xóa mất run history; job không dùng nữa được chuyển sang `DISABLED`. Foundation chưa tạo run, claim lock hoặc thực thi handler.

## Markdown and embedded media contract

- `description_markdown`, `content_markdown`, và `body_markdown` lưu Markdown source, không lưu HTML không được kiểm soát.
- API chỉ render Markdown bằng sanitizer/allowlist ở client hoặc renderer; không cho phép raw HTML và script.
- Media được nhúng bằng public/proxy URL do Media Service cấp, ví dụ `![Sơ đồ](/api/media/{mediaId}/content)`.
- Sau khi owner tạo hoặc cập nhật Markdown, owner service gọi Media Service để đăng ký/xóa `media_usages`; Media Service xác thực `media_id` và ownership trước khi tạo usage.

## Physical schema migration

`V001` sạch của từng service tạo các bảng ở trên bằng MySQL InnoDB, dùng `CHAR(36)` cho UUID và `DATETIME(6)` theo UTC. Foreign key chỉ tồn tại giữa bảng trong cùng service database:

- Course: `lessons.course_id`, `enrollments.course_id`, `lesson_progresses.lesson_id`.
- Media: `media_usages.media_id`.
- Notification: các liên kết giữa Batch, Batch Item và Notification.
- Scheduler: `background_job_runs.background_job_id`.

`student_id`, `course_id`, `uploaded_by`, `created_by`, `recipient_student_id`, và các owner ID từ service khác chỉ là logical reference, không có cross-database FK.

Để đảm bảo chỉ một thumbnail Course còn hiệu lực, `media_usages` có generated column kỹ thuật `active_course_thumbnail_owner_id`. Giá trị này chỉ có khi `owner_type = COURSE_THUMBNAIL` và `deleted_at IS NULL`; unique index trên cột này chặn thumbnail active thứ hai cho cùng Course.
