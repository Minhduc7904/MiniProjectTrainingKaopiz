# `GET /course/api/courses/summary`

Business flow: [Lấy tổng Course và Lesson](../../../business-flows/courses/get-courses-summary.md).

## Mục đích

Course Service trả tổng row `courses` và `lessons` cho dashboard quản trị.
Direct path là `GET /api/courses/summary`.

## Xác thực và phân quyền

Yêu cầu `X-Actor-Type: ADMIN` và `X-Actor-Id` là UUID hợp lệ.

## Phản hồi thành công

`200 OK`, `Cache-Control: no-store`.

```json
{ "data": { "totalCourses": 42, "totalLessons": 178 }, "meta": { "traceId": "01J..." } }
```

## Mã trạng thái HTTP

- `200`: Đọc toàn bộ Course và Lesson, không lọc status.
- `400 VALIDATION_FAILED`: Actor header không hợp lệ.
- `403 FORBIDDEN`: Actor không phải ADMIN.
- `500`: Lỗi không mong đợi khi đọc database.

Endpoint safe, idempotent, không có body và không có tác động phụ.
