# `GET /course/api/student/enrollments`

Business flow: [`get-student-enrollments.md`](../../../business-flows/course-learning/get-student-enrollments.md).

## Mục đích

Trả về các Course mà Student hiện tại đã ghi danh để hiển thị Home Student.

## Xác thực và phân quyền

- Actor header bắt buộc: `X-Actor-Type: STUDENT`, `X-Actor-Id` là UUID hợp lệ.
- Course Service luôn lọc theo `student_id` từ actor; client không được truyền `studentId`.

## Yêu cầu

Không có request body.

| Query | Kiểu | Mặc định | Quy tắc |
| --- | --- | --- | --- |
| `page` | integer | `1` | >= 1. |
| `pageSize` | integer | `12` | 1–50. |

Kết quả luôn có thứ tự `enrolled_at DESC, enrollment.id DESC`; đây là total order ổn định cho offset pagination.

## Phản hồi thành công

```http
200 OK
Cache-Control: no-store
```

```json
{
  "data": [{
    "enrollmentId": "33333333-3333-3333-3333-333333333333",
    "courseId": "11111111-1111-1111-1111-111111111111",
    "name": "Backend Fundamentals",
    "status": "PUBLISHED",
    "createdAtUtc": "2026-08-01T00:00:00Z",
    "enrolledAtUtc": "2026-08-20T00:00:00Z",
    "thumbnailUrl": "/media/api/media/.../content"
  }],
  "meta": { "traceId": "01J...", "pagination": { "type": "offset", "page": 1, "pageSize": 12, "totalItems": 1, "totalPages": 1 } }
}
```

`thumbnailUrl` có thể là `null` khi Course chưa có thumbnail. Không có Course ở trang yêu cầu vẫn trả `200` với `data: []` và metadata hợp lệ.

## Mã trạng thái HTTP

- `200`: lấy trang ghi danh thành công.
- `400 VALIDATION_FAILED`: `page` hoặc `pageSize` không hợp lệ.
- `403 FORBIDDEN`: actor không phải Student.

## Điều kiện nghiệp vụ và tác động phụ

Endpoint chỉ đọc `enrollments` và `courses`, sau đó đọc metadata thumbnail từ Media Service theo batch. Không tạo side effect; count và dữ liệu trang là snapshot độc lập theo semantics offset pagination.

## Đồng bộ artifact

- Postman: `CourseService/GET Student enrollments`.
- Tests: `StudentLearningHandlersTests`, `StudentLearningEndpointsComponentTests`.
