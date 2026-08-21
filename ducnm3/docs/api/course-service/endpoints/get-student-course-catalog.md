# `GET /course/api/student/courses`

Business flow: [`get-student-course-catalog.md`](../../../business-flows/course-learning/get-student-course-catalog.md).

## Mục đích

Trả danh mục Course `PUBLISHED` mà Student hiện tại chưa ghi danh để hiển thị trang Khóa học.

## Xác thực và phân quyền

- Actor header bắt buộc: `X-Actor-Type: STUDENT`, `X-Actor-Id` là UUID hợp lệ.
- Service tự áp điều kiện chưa ghi danh theo actor; client không truyền `studentId`.

## Yêu cầu

Không có request body.

| Query | Kiểu | Mặc định | Quy tắc |
| --- | --- | --- | --- |
| `page` | integer | `1` | >= 1. |
| `pageSize` | integer | `12` | 1–50. |

Kết quả dùng total order `courses.created_at DESC, courses.id DESC`. Chỉ Course `PUBLISHED` và không có enrollment `(course_id, student_id)` của actor được trả về.

## Phản hồi thành công

```http
200 OK
Cache-Control: no-store
```

```json
{
  "data": [{
    "courseId": "11111111-1111-1111-1111-111111111111",
    "name": "Backend Fundamentals",
    "status": "PUBLISHED",
    "createdAtUtc": "2026-08-01T00:00:00Z",
    "thumbnailUrl": "/media/api/media/.../content"
  }],
  "meta": { "traceId": "01J...", "pagination": { "type": "offset", "page": 1, "pageSize": 12, "totalItems": 1, "totalPages": 1 } }
}
```

`thumbnailUrl` có thể là `null`. Trang không có Course trả `200` với `data: []` và metadata offset hợp lệ.

## Mã trạng thái HTTP

- `200`: lấy danh mục thành công.
- `400 VALIDATION_FAILED`: `page` hoặc `pageSize` không hợp lệ.
- `403 FORBIDDEN`: actor không phải Student.

## Điều kiện nghiệp vụ và tác động phụ

Endpoint chỉ đọc `courses`, `enrollments` và metadata thumbnail batch từ Media Service. Index hiện có `ix_courses_status_created_at` và `uq_enrollments_course_id_student_id` hỗ trợ filter/sort/exclusion; không cần migration. Không có side effect.

## Đồng bộ artifact

- Postman: `CourseService/GET Student course catalog`.
- Tests: `StudentLearningHandlersTests`, `StudentLearningEndpointsComponentTests`.
