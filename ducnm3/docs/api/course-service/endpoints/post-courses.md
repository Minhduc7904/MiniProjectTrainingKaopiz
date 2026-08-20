# Tạo khóa học

Business flow: [Tạo khóa học](../../../business-flows/courses/post-courses.md).

`POST /course/api/courses` tạo một Course mới. Direct path của Course Service là
`POST /api/courses`.

## Request

Header bắt buộc: `X-Actor-Id` là UUID khác `00000000-0000-0000-0000-000000000000`.

```json
{
  "name": "Backend Fundamentals",
  "descriptionMarkdown": "![Bìa khóa học](/media/api/media/11111111-1111-1111-1111-111111111111/content)"
}
```

| Field | Bắt buộc | Quy tắc |
| --- | --- | --- |
| `name` | Có | Chuỗi sau khi trim dài 3–200 ký tự. |
| `descriptionMarkdown` | Không | Markdown; `null`, rỗng hoặc chỉ có khoảng trắng được lưu là `null`. |

`status` không thuộc request contract. API luôn tạo Course với `status: "DRAFT"`;
nếu client gửi thêm field `status`, field đó bị bỏ qua và không thể làm Course mới
thành `PUBLISHED` hay `ARCHIVED`.

Để đồng bộ media, dùng URL public của Media Service trong Markdown theo mẫu
`/media/api/media/{mediaId}/content`. Image Markdown (`![](...)`) là usage
`EMBED`; link Markdown (`[](...)`) là usage `ATTACHMENT`. Các URL không đúng mẫu
vẫn là nội dung Markdown thông thường và không tạo media usage.

## Response

Trả `201 Created`, header `Location` trỏ tới
`/course/api/courses/{courseId}/details`, và response envelope chuẩn:

```json
{
  "data": {
    "id": "course-uuid",
    "name": "Backend Fundamentals",
    "descriptionMarkdown": "![Bìa khóa học](/media/api/media/11111111-1111-1111-1111-111111111111/content)",
    "status": "DRAFT",
    "createdAtUtc": "2026-08-20T06:00:00Z",
    "updatedAtUtc": "2026-08-20T06:00:00Z"
  },
  "meta": { "traceId": "01J..." }
}
```

## Xử lý media và retry

Course được lưu trước, sau đó Application gửi command
`SynchronizeCourseContentMediaUsageV1` tới Media Service cho các media reference
trong `descriptionMarkdown`. HTTP chỉ chờ command được nhận bởi transport, không
chờ Media worker xử lý xong; worker đồng bộ usage `COURSE_DESCRIPTION` theo cơ chế
idempotent.

Endpoint chưa có idempotency key hoặc kho lưu replay. Client không được tự động
retry POST sau khi timeout mà không có xác nhận, vì mỗi lần gọi thành công có thể
tạo một Course `DRAFT` mới.

## Lỗi

- `400 VALIDATION_FAILED`: thiếu/không hợp lệ `X-Actor-Id`, hoặc `name` rỗng hay
  không dài 3–200 ký tự.
- `500`: không thể hoàn tất xử lý nội bộ, gồm cả lỗi gửi command đến Media Service
  sau khi Course đã được lưu; client cần tra `traceId` trước khi quyết định xử lý
  tiếp.
