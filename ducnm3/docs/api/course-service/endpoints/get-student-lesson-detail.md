# `GET /course/api/student/enrollments/{courseId}/lessons/{lessonId}`

Business flow: [`get-student-lesson-detail.md`](../../../business-flows/course-learning/get-student-lesson-detail.md).

## Mục đích

Course Service trả nội dung đầy đủ của một Lesson cho chính Student đã ghi danh Course: Markdown gốc, HTML đã sanitize và attachment/media usage. Direct service path là `GET /api/student/enrollments/{courseId}/lessons/{lessonId}`.

## Xác thực và phân quyền

- Actor bắt buộc là Student.
- Service xác minh enrollment theo `courseId` và `X-Actor-Id` trước khi đọc Lesson hoặc Media metadata.

## Yêu cầu

| Tham số | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `courseId` | UUID | Có | UUID hợp lệ, khác rỗng. |
| `lessonId` | UUID | Có | UUID hợp lệ, khác rỗng và thuộc Course. |

Không có query parameter hay request body.

## Phản hồi thành công

`200 OK`, `Cache-Control: no-store`.

```json
{
  "data": {
    "id": "22222222-2222-2222-2222-222222222222",
    "courseId": "11111111-1111-1111-1111-111111111111",
    "title": "Bắt đầu",
    "contentMarkdown": "# Bắt đầu",
    "contentHtml": "<h1>Bắt đầu</h1>",
    "displayOrder": 1,
    "createdAtUtc": "2026-08-01T00:00:00Z",
    "updatedAtUtc": "2026-08-01T00:00:00Z",
    "attachments": [{ "usageId": "33333333-3333-3333-3333-333333333333", "contentUrl": "/media/api/media/.../content", "thumbnailUrl": null, "mediaType": "IMAGE", "contentType": "image/jpeg", "originalFileName": "lesson.jpg" }]
  },
  "meta": { "traceId": "01J..." }
}
```

`contentHtml` do Course Service render bằng Markdig và sanitize; frontend không render Markdown thô thành HTML.

## Mã trạng thái HTTP

- `200`: Student đã ghi danh và Lesson thuộc Course.
- `400 VALIDATION_FAILED`: `courseId` hoặc `lessonId` không hợp lệ.
- `403 STUDENT_NOT_ENROLLED`: actor không phải Student hoặc chưa ghi danh Course.
- `404 LESSON_NOT_FOUND`: Lesson không tồn tại hoặc không thuộc Course.

Lỗi dùng [error envelope chuẩn](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

Request safe, idempotent và chỉ đọc `lessons` của Course Service cùng Media metadata qua `ICourseMediaReader`. Không thay đổi database, publish message hoặc tạo job.
