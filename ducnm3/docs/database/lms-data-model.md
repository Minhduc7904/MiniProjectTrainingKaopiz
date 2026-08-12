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

### notification_jobs

Chỉ dùng cho gửi hàng loạt; một job tạo notification riêng cho từng recipient.

```text
id                    // UUID định danh batch job
course_id             // UUID Course liên quan; nullable nếu không gửi theo Course
title                 // Tiêu đề thông báo
body_markdown         // Nội dung Markdown; có thể nhúng media qua URL của Media Service
target_scope          // COURSE_ENROLLED | STUDENT_IDS | ALL_STUDENTS
created_by            // UUID admin tạo job
status                // PENDING | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED
total_count           // Tổng recipient đã snapshot khi tạo job
processed_count       // Số recipient worker đã xử lý
success_count         // Số notification tạo thành công
failed_count          // Số recipient thất bại sau retry
batch_size            // Số item xử lý trên mỗi chunk
started_at            // Thời điểm worker bắt đầu; nullable khi job chưa chạy
completed_at          // Thời điểm job kết thúc; nullable khi chưa hoàn tất
created_at            // Thời điểm tạo job, UTC
```

### notification_job_items

```text
id                    // UUID định danh recipient trong batch
job_id                // UUID notification_jobs.id
student_id            // UUID Student nhận thông báo; logical reference
notification_id       // UUID notifications.id được tạo; nullable khi chưa thành công
status                // PENDING | PROCESSING | SUCCESS | RETRY | FAILED
retry_count           // Số lần retry đã thực hiện
error_message         // Lỗi cuối cùng; nullable khi thành công
processed_at          // Thời điểm xử lý thành công hoặc thất bại cuối; nullable khi chưa xử lý
```

```sql
UNIQUE(job_id, student_id)
```

### notifications

Mỗi bản ghi là một item inbox của một Student. Gửi đơn và gửi hàng loạt đều dùng cùng bảng này.

```text
id                    // UUID định danh inbox item
recipient_student_id  // UUID Student sở hữu notification; dùng để kiểm tra ownership
title                 // Tiêu đề hiển thị trong inbox
body_markdown         // Nội dung Markdown; media nhúng dùng URL của Media Service
source_type           // SINGLE | BULK
notification_job_id   // UUID notification_jobs.id; nullable với gửi đơn
created_by            // UUID admin hoặc system tạo notification
status                // UNREAD | READ
read_at               // Thời điểm recipient đánh dấu đã đọc; nullable khi UNREAD
created_at            // Thời điểm notification xuất hiện trong inbox, UTC
```

Index và ràng buộc:

```sql
INDEX(recipient_student_id, status, created_at DESC)
UNIQUE(notification_job_id, recipient_student_id)
```

## Markdown and embedded media contract

- `description_markdown`, `content_markdown`, và `body_markdown` lưu Markdown source, không lưu HTML không được kiểm soát.
- API chỉ render Markdown bằng sanitizer/allowlist ở client hoặc renderer; không cho phép raw HTML và script.
- Media được nhúng bằng public/proxy URL do Media Service cấp, ví dụ `![Sơ đồ](/api/media/{mediaId}/content)`.
- Sau khi owner tạo hoặc cập nhật Markdown, owner service gọi Media Service để đăng ký/xóa `media_usages`; Media Service xác thực `media_id` và ownership trước khi tạo usage.
