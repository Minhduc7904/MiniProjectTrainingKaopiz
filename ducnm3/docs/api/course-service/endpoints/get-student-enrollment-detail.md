# `GET /course/api/student/enrollments/{courseId}`

Business flow: [`get-student-enrollment-detail.md`](../../../business-flows/course-learning/get-student-enrollment-detail.md).

## Mục đích

Trả về bản xem chỉ đọc của Course đã ghi danh, gồm thumbnail gốc, gallery, mô tả HTML đã sanitize và danh sách Lesson preview.

## Xác thực và phân quyền

- Actor bắt buộc là Student.
- Dịch vụ kiểm tra enrollment theo `courseId` và `X-Actor-Id` trước khi đọc Course. Không dùng endpoint Course detail quản trị vì endpoint đó có dữ liệu progress của tất cả Student.

## Yêu cầu

`courseId` là UUID khác rỗng; không có request body.

## Phản hồi thành công

```http
200 OK
Cache-Control: no-store
```

```json
{
  "data": {
    "id": "11111111-1111-1111-1111-111111111111",
    "name": "Backend Fundamentals",
    "descriptionHtml": "<p>Nội dung đã được sanitize.</p>",
    "status": "PUBLISHED",
    "createdAtUtc": "2026-08-01T00:00:00Z",
    "thumbnail": { "contentUrl": "/media/api/media/.../content" },
    "gallery": [],
    "lessons": [{ "id": "22222222-2222-2222-2222-222222222222", "title": "Bắt đầu", "displayOrder": 1, "progressPercent": 100, "completedAtUtc": "2026-08-22T00:00:00Z" }]
  },
  "meta": { "traceId": "01J..." }
}
```

`thumbnail` có thể `null`; mỗi Lesson chỉ có preview, progress của current Student và không trả nội dung Lesson hoặc progress của Student khác.

## Mã trạng thái HTTP

- `200`: Student có enrollment và Course tồn tại.
- `400 VALIDATION_FAILED`: `courseId` không hợp lệ.
- `403 STUDENT_NOT_ENROLLED`: actor chưa ghi danh Course hoặc actor không phải Student.
- `404 COURSE_NOT_FOUND`: enrollment hợp lệ nhưng Course không còn tồn tại.

## Điều kiện nghiệp vụ và tác động phụ

Endpoint chỉ đọc Course, Lesson và Media metadata. Markdown mô tả được render rồi sanitize trước response. Công thức `$...$`, `$$...$$` và `\\[...\\]` được thay bằng placeholder `course-math` an toàn để frontend typeset KaTeX với `trust: false`. Không có side effect.

## Đồng bộ artifact

- Postman: `CourseService/GET Student enrollment detail`.
- Tests: `StudentLearningHandlersTests`, `StudentLearningEndpointsComponentTests`.
