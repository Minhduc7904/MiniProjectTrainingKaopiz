# 15. API tối thiểu cần implement

## Course Service

```http
POST   /api/courses
GET    /api/courses/{id}
GET    /api/courses
GET    /api/courses/cursor
POST   /api/courses/{id}/lessons
GET    /api/courses/{id}/details
GET    /api/courses/export
```

## Student Service

```http
POST /api/students/seed
GET  /api/students
GET  /api/students/{id}
GET  /api/students/course/{courseId}
```

## Media Service

```http
POST   /api/media
GET    /api/media/{id}
GET    /api/media/{id}/content
DELETE /api/media/{id}
POST   /api/media/usages
DELETE /api/media/usages/{id}
GET    /api/media/usages
```

- `POST /api/media` nhận multipart upload và metadata (`mediaType`, `contentType`, `sizeBytes`); chỉ Media Service gọi MinIO.
- `GET /api/media/{id}/content` trả proxy stream hoặc redirect presigned URL; Markdown chỉ nhúng endpoint này, không nhúng `object_key`.
- `POST /api/media/usages` đăng ký owner (`ownerService`, `ownerType`, `ownerId`) và cách dùng (`THUMBNAIL`, `EMBED`, `ATTACHMENT`).
- `GET /api/media/usages` nhận `ownerService`, `ownerType`, `ownerId` để lấy thumbnail hoặc danh sách media của một nội dung.
- Course Service và Notification Service gọi các API này; không có media endpoint hoặc truy cập MinIO/database media trực tiếp.

## Notification Service

```http
POST /api/notifications
POST /api/notification-jobs
GET  /api/notification-jobs/{id}
GET  /api/notification-jobs/{id}/failed-items
GET  /api/notifications/me
PATCH /api/notifications/{id}/read
POST /api/notifications/read-all
```

Quy ước:

- `POST /api/notifications` gửi một thông báo cho một `studentId`.
- `POST /api/notification-jobs` gửi hàng loạt, trả `202 Accepted`, và được worker xử lý bất đồng bộ.
- `GET /api/notifications/me` chỉ trả inbox của user đang xác thực; hỗ trợ lọc `status=UNREAD` và cursor pagination.
- `PATCH /api/notifications/{id}/read` phải kiểm tra ownership trước khi cập nhật `read_at`.
- Course `descriptionMarkdown`, Lesson `contentMarkdown`, và Notification `bodyMarkdown` nhận Markdown source. Raw HTML/script bị cấm và phải được sanitize khi render.
- Media nhúng trong Markdown dùng URL do Media Service cấp, ví dụ `![Tài liệu](/api/media/{mediaId}/content)`.

---
# 16. Mapping yêu cầu lead → Project

| Yêu cầu | Use case demo |
|---|---|
| Docker | Dockerize Gateway + 4 services + MySQL + MinIO |
| MinIO | Media Service upload và stream thumbnail/document/media |
| Batch Job | Broadcast notification cho 3k/10k/100k student |
| Batch Retry | Fake sender random fail → retry 1 lần |
| Batch Performance | Đo time + memory 3k/10k/100k |
| CSV Export | Export 100k+ Course |
| CSV Performance | Load All vs Streaming |
| N+1 | Course → Lessons → Progress |
| Index | Search Course trên 10k/100k/1M |
| Query Plan | EXPLAIN ANALYZE trước/sau |
| Pagination | OFFSET vs Cursor |
| API Performance | Benchmark endpoint trước/sau |
| Error Handling | 400 / 404 / 409 / 500 |

---
