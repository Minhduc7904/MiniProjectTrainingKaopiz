# Mô hình dữ liệu LMS, Media và Thông báo

Mỗi dịch vụ sở hữu cơ sở dữ liệu riêng. Các ID tham chiếu sang dịch vụ khác chỉ là tham chiếu logic; không tạo khóa ngoại xuyên cơ sở dữ liệu.

## Cơ sở dữ liệu Course Service

### courses

```text
id                    // UUID định danh Khóa học
name                  // Tên Khóa học hiển thị cho người dùng
description_markdown  // Nội dung mô tả Markdown; có thể nhúng media qua URL của Media Service
status                // DRAFT | PUBLISHED | ARCHIVED
created_at            // Thời điểm tạo Khóa học, UTC
updated_at            // Thời điểm cập nhật gần nhất, UTC
```

### lessons

```text
id                    // UUID định danh Bài học
course_id             // UUID Khóa học sở hữu Bài học, trong cùng cơ sở dữ liệu Course Service
title                 // Tiêu đề Bài học
display_order         // Thứ tự hiển thị Bài học trong Khóa học
content_markdown      // Nội dung Markdown; có thể nhúng media qua URL của Media Service
created_at            // Thời điểm tạo Bài học, UTC
updated_at            // Thời điểm cập nhật gần nhất, UTC
```

### enrollments

```text
id                    // UUID định danh lượt ghi danh
course_id             // UUID Khóa học được ghi danh
student_id            // UUID Học viên từ Student Service; tham chiếu logic, không có khóa ngoại xuyên CSDL
enrolled_at           // Thời điểm Học viên ghi danh, UTC
```

### lesson_progresses

```text
id                    // UUID định danh tiến độ học
lesson_id             // UUID Bài học được theo dõi tiến độ
student_id            // UUID Học viên từ Student Service; tham chiếu logic
progress_percent      // Phần trăm hoàn thành, 0–100
completed_at          // Thời điểm hoàn thành; có thể null khi chưa hoàn thành
updated_at            // Thời điểm cập nhật tiến độ gần nhất, UTC
```

## Cơ sở dữ liệu Student Service

### students

```text
id                    // UUID định danh Học viên
email                 // Email đăng nhập hoặc liên hệ; duy nhất
display_name          // Tên hiển thị của Học viên
status                // ACTIVE | INACTIVE | BLOCKED
created_at            // Thời điểm tạo Học viên, UTC
updated_at            // Thời điểm cập nhật Học viên gần nhất, UTC
```

Chỉ mục và ràng buộc:

```sql
UNIQUE(email)
INDEX(status, created_at DESC)
```

## Tập dữ liệu mẫu cho môi trường phát triển

`Lms.DataSeeder` chỉ nạp dữ liệu vào các cơ sở dữ liệu Student Service và Course Service hiện có
khi nhà phát triển chủ động chạy công cụ. Dữ liệu mặc định:

- 100k `students`;
- 100k `courses`;
- 1–5 `lessons` cho mỗi Khóa học;
- 1–10 `enrollments` cho mỗi Học viên;
- không có `lesson_progresses`.

Các hàng dữ liệu mẫu dùng UUID xác định được trước, sinh từ một giá trị khởi tạo ngẫu nhiên
có thể cấu hình. `enrollments.student_id` dùng các UUID Học viên đó làm tham chiếu
logic, trong khi quy tắc không tạo khóa ngoại xuyên cơ sở dữ liệu vẫn giữ nguyên.
Dữ liệu mẫu không phải trạng thái lược đồ và không bao giờ được ghi vào
`schema_migrations`; thao tác đặt lại cơ sở dữ liệu phát triển có bảo vệ sẽ xóa
dữ liệu này cùng mọi hàng khác.

## Cơ sở dữ liệu Media Service

### media_objects

Lưu siêu dữ liệu cho đối tượng trong MinIO; không lưu dữ liệu nhị phân trong MySQL.

```text
id                    // UUID định danh media
source_media_id       // Media gốc của object dẫn xuất; null với upload gốc
derivation_type       // THUMBNAIL với object dẫn xuất; null với upload gốc
bucket                // Tên vùng chứa MinIO chứa đối tượng
object_key            // Khóa đối tượng duy nhất trong vùng chứa; không trả trực tiếp cho ứng dụng khách
media_type            // IMAGE | VIDEO | DOCUMENT | AUDIO | OTHER
content_type          // Loại MIME đã xác thực, ví dụ image/webp hoặc application/pdf
original_file_name    // Tên tệp do người dùng tải lên, chỉ để hiển thị
size_bytes            // Kích thước đối tượng theo byte
checksum_sha256       // Mã băm SHA-256; null khi upload chưa READY
uploaded_by           // UUID actor tải media lên; logical reference
uploaded_by_type      // Actor type đã được Application validate; hiện là STUDENT
status                // PENDING | READY | FAILED
failure_reason        // Lỗi nội bộ an toàn khi FAILED; không trả cho client
completed_at          // Thời điểm chuyển READY, UTC; null nếu chưa hoàn tất
created_at            // Thời điểm tạo media record, UTC
updated_at            // Thời điểm media record cập nhật gần nhất, UTC
deleted_at            // Thời điểm xóa mềm; có thể null khi media còn hoạt động
```

Chỉ mục và ràng buộc:

```sql
UNIQUE(bucket, object_key)
INDEX(uploaded_by, created_at DESC)
INDEX(status, created_at)
CHECK(status IN ('PENDING', 'READY', 'FAILED'))
INDEX(source_media_id, derivation_type)
```

Luồng upload là database-first: service ghi hàng `PENDING` trước khi stream
object vào MinIO. Adapter tính checksum SHA-256 trong lúc upload; khi thành công,
service lưu checksum và chuyển `READY`. Nếu upload, checksum hoặc bước hoàn tất
database lỗi, service cố gắng xóa object và chuyển hàng sang `FAILED`.
Compensation là best effort; quét các hàng `PENDING` stale sau sự cố process được
hoãn cho Scheduler và chưa được triển khai.

### media_derivation_jobs

Theo dõi operation bất đồng bộ tạo thumbnail, tách biệt với `media_objects`
chứa metadata file WebP:

```text
id                    // UUID job
source_media_id       // Media gốc READY
derivative_media_id   // Media WebP PENDING/READY/FAILED được cấp trước
derivation_type       // THUMBNAIL
status                // QUEUED | PROCESSING | READY | FAILED
attempt_count         // Số lần worker bắt đầu xử lý
last_error            // Lỗi an toàn cuối cùng
started_at/completed_at/created_at/updated_at
```

Mỗi `(source_media_id, derivation_type)` và mỗi `derivative_media_id` là duy
nhất. Job cho phép query trạng thái, retry idempotent và không phụ thuộc khả năng
quan sát queue RabbitMQ.

Ba bảng MassTransit `OutboxMessage`, `OutboxState`, `InboxState` bảo đảm command
được ghi cùng transaction upload/retry và consumer không áp dụng thành công một
message nhiều lần.

### media_usages

Bảng này liên kết một media với vị trí sử dụng mà không để Course Service hoặc Notification Service sở hữu siêu dữ liệu media.

```text
id                    // UUID định danh liên kết sử dụng
media_id              // UUID media_objects.id trong cơ sở dữ liệu Media Service
owner_service         // COURSE | NOTIFICATION | STUDENT | MEDIA
owner_type            // ... | STUDENT_AVATAR | MEDIA_THUMBNAIL
owner_id              // UUID tài nguyên ở owner_service; tham chiếu logic
usage_type            // THUMBNAIL | EMBED | ATTACHMENT | AVATAR
display_order         // Thứ tự hiển thị media trong cùng một đối tượng sở hữu
created_by            // UUID actor tạo liên kết; logical reference
created_by_type       // Actor type đã được Application validate; STUDENT hoặc ADMIN
created_at            // Thời điểm tạo liên kết, UTC
deleted_at            // Thời điểm xóa mềm; có thể null khi lượt sử dụng còn hiệu lực
active_reference_guard // Generated 1 khi active, null khi đã soft-delete
active_media_thumbnail_owner_id // Generated owner_id của thumbnail media active
```

Chỉ mục và ràng buộc:

```sql
UNIQUE(media_id, owner_service, owner_type, owner_id, usage_type, active_reference_guard)
INDEX(owner_service, owner_type, owner_id, display_order)
```

`active_reference_guard` cho phép cùng media được dùng lại sau khi usage cũ đã
soft-delete, nhưng vẫn chặn hai reference giống hệt nhau cùng active.

Quy tắc `THUMBNAIL`:

- `COURSE_THUMBNAIL` là nguồn dữ liệu chuẩn duy nhất cho ảnh đại diện; bảng `courses` không lưu `thumbnail_media_id`.
- Media Service chỉ cho phép tối đa một `media_usages` đang hoạt động có `owner_type = COURSE_THUMBNAIL` cho mỗi `owner_id`.
- Khi thay ảnh đại diện, Media Service xóa mềm lượt sử dụng cũ và tạo lượt sử dụng mới trong cùng một giao dịch.
- Với `MEDIA/MEDIA_THUMBNAIL/THUMBNAIL`, `owner_id` là media gốc và `media_id`
  là WebP dẫn xuất `READY`. Generated unique guard bảo đảm tối đa một thumbnail
  active; thay thumbnail soft-delete usage cũ trong transaction.

Quy tắc `AVATAR` hiện đã triển khai:

- Command tạo usage chấp nhận avatar tuple và media-thumbnail tuple.
- Chỉ media `READY`, chưa bị xóa mềm mới được dùng.
- Media Service xóa mềm avatar active cũ và tạo usage mới trong transaction
  `SERIALIZABLE`.
- Generated column `active_student_avatar_owner_id` chỉ nhận `owner_id` cho
  usage `STUDENT/STUDENT_AVATAR/AVATAR` chưa bị xóa. Unique index trên cột này
  bảo đảm tối đa một avatar active cho mỗi Học viên.
- `created_by_type/created_by` là actor thực hiện request, tách biệt với
  `owner_service/owner_type/owner_id`. Actor fields hiện do request chưa xác thực
  cung cấp tạm thời và sẽ được ánh xạ từ JWT claim sau này.

## Cơ sở dữ liệu Notification Service

Notification Service lưu nội dung và bản chụp danh sách người nhận của quá trình phân phối hàng loạt. Đây là dữ liệu nghiệp vụ, không phải siêu dữ liệu lịch chạy dùng chung của Scheduler.

### notification_batches

```text
id                    // UUID định danh yêu cầu gửi hàng loạt
course_id             // UUID Khóa học liên quan; có thể null nếu phạm vi không theo Khóa học
title                 // Tiêu đề thông báo dùng cho toàn lô
body_markdown         // Nội dung Markdown dùng cho toàn lô
target_scope          // COURSE_ENROLLED | STUDENT_IDS | ALL_STUDENTS
created_by            // UUID quản trị viên tạo lô; tham chiếu logic
status                // PENDING | SNAPSHOTTING | SNAPSHOT_READY | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED
total_count           // Tổng số người nhận đã được chụp khi tạo lô
processed_count       // Số người nhận đã được xử lý
success_count         // Số mục hộp thư đến được tạo thành công
failed_count          // Số người nhận thất bại sau khi thử lại
batch_size            // Số người nhận nghiệp vụ được xử lý trong mỗi nhóm
started_at            // Thời điểm bắt đầu xử lý, UTC; có thể null khi chưa chạy
completed_at          // Thời điểm kết thúc, UTC; có thể null khi chưa hoàn tất
created_at            // Thời điểm tạo lô, UTC
```

Vòng đời trạng thái dự kiến: `PENDING -> PROCESSING -> COMPLETED | PARTIAL_FAILED | FAILED`. Phần nền tảng hiện chỉ có lược đồ; chưa có bộ xử lý chuyển trạng thái.

### notification_batch_items

```text
id                    // UUID định danh người nhận trong lô
batch_id              // UUID notification_batches.id trong cùng cơ sở dữ liệu
student_id            // UUID Học viên nhận thông báo; tham chiếu logic
notification_id       // UUID notifications.id được tạo; có thể null khi chưa thành công
status                // PENDING | PROCESSING | SUCCESS | RETRY | FAILED
retry_count           // Số lần thử lại mục nghiệp vụ đã thực hiện
error_message         // Lỗi cuối cùng; có thể null khi chưa lỗi hoặc đã thành công
processed_at          // Thời điểm xử lý cuối, UTC; có thể null khi chưa xử lý
```

`UNIQUE(batch_id, student_id)` ngăn chụp trùng người nhận khi Worker retry/redelivery. Khi xóa lô, các mục bị xóa theo; khi xóa mục hộp thư đến, `notification_id` của mục chỉ được đặt thành null.

### MassTransit outbox/inbox

`InboxState`, `OutboxState` và `OutboxMessage` là bảng hạ tầng do Notification Service sở hữu. Chúng bảo đảm `SnapshotNotificationBatchV1` và các command tiếp theo được ghi bền cùng transaction nghiệp vụ, đồng thời consumer deduplicate delivery at-least-once. Các bảng này không chứa dữ liệu notification hiển thị cho người dùng.

### notifications

```text
id                    // UUID định danh mục hộp thư đến
recipient_student_id  // UUID Học viên sở hữu thông báo; tham chiếu logic
title                 // Tiêu đề hiển thị trong hộp thư đến
body_markdown         // Nội dung Markdown; media nhúng dùng URL Media Service
source_type           // SINGLE | BULK
notification_batch_id // UUID notification_batches.id; có thể null với SINGLE
created_by            // UUID quản trị viên hoặc hệ thống tạo thông báo; tham chiếu logic
status                // UNREAD | READ
read_at               // Thời điểm đánh dấu đã đọc, UTC; có thể null khi UNREAD
created_at            // Thời điểm thông báo xuất hiện trong hộp thư đến, UTC
```

`UNIQUE(notification_batch_id, recipient_student_id)` ngăn tạo trùng mục hộp thư đến khi thử lại. `notification_batch_id` dùng `ON DELETE RESTRICT` để không làm mất nguồn kiểm toán của hộp thư đến `BULK`. Vòng đời đọc là `UNREAD -> READ`; `read_at` phải nhất quán với trạng thái.

## Cơ sở dữ liệu Scheduler Service

Scheduler Service chỉ sở hữu định nghĩa tác vụ dùng chung và lịch sử lượt chạy. Dịch vụ này không lưu nội dung thông báo, danh sách người nhận, siêu dữ liệu media và không có khóa ngoại tới cơ sở dữ liệu của dịch vụ khác.

### background_jobs

```text
id                    // UUID định danh cấu hình tác vụ nền
job_key               // Khóa ổn định và duy nhất để đăng ký/tra cứu tác vụ
name                  // Tên hiển thị cho vận hành
job_type              // Loại bộ xử lý tương lai, ví dụ NOTIFICATION_BATCH_DISPATCH
target_service        // Dịch vụ nghiệp vụ sẽ được gọi trong tương lai; giá trị logic
schedule_type         // MANUAL | CRON
cron_expression       // Biểu thức CRON theo UTC; bắt buộc với CRON, null với MANUAL
payload_json          // Cấu hình đầu vào nhỏ; không chứa danh sách người nhận/dữ liệu miền lớn
status                // ACTIVE | PAUSED | DISABLED
allow_concurrent      // Cho phép nhiều lượt chạy đồng thời của cùng tác vụ
max_retry_count       // Số lần thử lại tối đa cho giai đoạn thực thi tương lai
timeout_seconds       // Thời gian chạy tối đa; phải lớn hơn 0
next_run_at           // Thời điểm UTC chạy CRON kế tiếp; có thể null khi chưa tính/MANUAL
created_by            // UUID tác nhân tạo tác vụ; có thể null với tác vụ do hệ thống định nghĩa
created_at            // Thời điểm tạo tác vụ, UTC
updated_at            // Thời điểm cập nhật tác vụ gần nhất, UTC
```

`UNIQUE(job_key)` chặn cấu hình trùng. Chỉ mục `(status, next_run_at)` phục vụ tra cứu tác vụ đến hạn trong tương lai. Phần nền tảng chưa phân tích CRON hoặc cập nhật `next_run_at`.

### background_job_runs

```text
id                    // UUID định danh một lần thực thi
background_job_id     // UUID background_jobs.id trong cùng cơ sở dữ liệu Scheduler
trigger_type          // MANUAL | CRON | RETRY
idempotency_key       // Khóa chống tạo trùng lượt chạy cho cùng tác vụ
payload_snapshot_json // Bản chụp dữ liệu đầu vào tại thời điểm tạo lượt chạy
attempt_number        // Lần thử hiện tại, bắt đầu từ 1
status                // QUEUED | RUNNING | SUCCEEDED | FAILED | CANCELLED | TIMED_OUT | SKIPPED
scheduled_at          // Thời điểm UTC lượt chạy được lên lịch/kích hoạt thủ công
started_at            // Thời điểm Worker bắt đầu, UTC; có thể null
finished_at           // Thời điểm Worker kết thúc, UTC; có thể null
worker_instance       // Định danh Worker xử lý; có thể null trước khi nhận xử lý
correlation_id        // ID nối log Scheduler với dịch vụ đích
error_code            // Mã lỗi ổn định cuối cùng; có thể null khi chưa lỗi
error_message         // Thông tin lỗi an toàn; không chứa thông tin xác thực
output_json           // Kết quả tóm tắt; không thay thế cơ sở dữ liệu miền
created_at            // Thời điểm tạo lượt chạy, UTC
```

`UNIQUE(background_job_id, idempotency_key)` bảo đảm tính lũy đẳng. Chỉ mục `(status, scheduled_at)` phục vụ việc nhận hàng đợi để xử lý và `(background_job_id, created_at DESC)` phục vụ lịch sử lượt chạy. Khóa ngoại chỉ tồn tại nội bộ Scheduler và dùng `ON DELETE RESTRICT` để không xóa mất lịch sử lượt chạy; tác vụ không còn dùng được chuyển sang `DISABLED`. Phần nền tảng chưa tạo lượt chạy, khóa nhận xử lý hoặc thực thi bộ xử lý.

## Hợp đồng Markdown và media nhúng

- `description_markdown`, `content_markdown` và `body_markdown` lưu mã nguồn Markdown, không lưu HTML không được kiểm soát.
- API chỉ hiển thị Markdown bằng bộ làm sạch/danh sách cho phép ở ứng dụng khách hoặc bộ hiển thị; không cho phép HTML thô và mã lệnh.
- Media được nhúng bằng URL công khai do Media Service cấp, ví dụ `![Sơ đồ](/media/api/media/{mediaId}/content)`.
- Notification Service ghi command Media vào transactional outbox cùng thay đổi notification. Media Worker xác thực `media_id` `READY` và tạo idempotent `media_usages` sau khi notification thành công; batch chỉ phát command cho item `SUCCESS`, gom tối đa 500 notification IDs / 1,000 usage rows để một media dùng chung được validate một lần nhưng vẫn có một usage row cho mỗi notification owner.

## Thay đổi lược đồ vật lý

`V001` sạch của từng dịch vụ tạo schema nền bằng MySQL InnoDB, dùng `CHAR(36)`
cho UUID và `DATETIME(6)` theo UTC. Media Service dùng `V002` để bổ sung vòng
đời upload, actor type và uniqueness avatar Học viên. Khóa ngoại chỉ tồn tại
giữa các bảng trong cùng database service:

- Course Service: `lessons.course_id`, `enrollments.course_id`, `lesson_progresses.lesson_id`.
- Media Service: `media_usages.media_id`.
- Notification Service: các liên kết giữa lô, mục trong lô và Thông báo.
- Scheduler Service: `background_job_runs.background_job_id`.

`student_id`, `course_id`, `uploaded_by`, `created_by`, `recipient_student_id` và các ID đối tượng sở hữu từ dịch vụ khác chỉ là tham chiếu logic, không có khóa ngoại xuyên cơ sở dữ liệu.

Để đảm bảo chỉ một ảnh đại diện Khóa học còn hiệu lực, `media_usages` có cột sinh kỹ thuật `active_course_thumbnail_owner_id`. Giá trị này chỉ có khi `owner_type = COURSE_THUMBNAIL` và `deleted_at IS NULL`; chỉ mục duy nhất trên cột này chặn ảnh đại diện đang hoạt động thứ hai cho cùng Khóa học.

Tương tự, `active_student_avatar_owner_id` chỉ có giá trị với usage
`STUDENT/STUDENT_AVATAR/AVATAR` active; unique index
`uq_media_usages_active_student_avatar` chặn avatar Học viên active thứ hai.
