# 17. Bài toán MinIO

Flow:

```text
Client
  │
  │ multipart/form-data
  ▼
Media Service
  │
  │ PutObject
  ▼
MinIO
  │
  └── bucket: course-assets
```

DB chỉ lưu:

```text
media_objects
id                  // UUID media do Media Service quản lý
bucket              // Bucket MinIO chứa object
object_key          // Khóa nội bộ, không trả trực tiếp cho client
media_type          // IMAGE | VIDEO | DOCUMENT | AUDIO | OTHER
content_type        // MIME type đã validate
size_bytes          // Kích thước file bằng byte
original_file_name  // Tên file để hiển thị
```

Ví dụ:

```text
bucket:
course-assets

object_key:
media/2026/08/2a96....webp
```

`media_type` có thể là `IMAGE`, `VIDEO`, `DOCUMENT`, `AUDIO` hoặc `OTHER`. Course và Notification Service chỉ lưu Markdown và gọi Media Service để tạo `media_usages`; MinIO chỉ giữ object, Media Service database giữ metadata và quan hệ usage.

Không lưu file binary vào MySQL.

---
