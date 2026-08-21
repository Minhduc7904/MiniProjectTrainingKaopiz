# `POST /student/api/auth/login`

Đăng nhập demo bằng UUID Student đã tồn tại; không cấp token/JWT. Client lưu
`actor: "STUDENT"` và `id` trả về vào Student local storage.

Business flow: [student-demo-auth.md](../../../business-flows/students/student-demo-auth.md).

## Request và response

```json
{ "id": "11111111-1111-1111-1111-111111111111" }
```

Thành công trả `200 OK` với:

```json
{ "data": { "actor": "STUDENT", "id": "student-uuid" }, "meta": { "traceId": "..." } }
```

- `400 VALIDATION_FAILED`: `id` không phải UUID hợp lệ hoặc rỗng.
- `404 STUDENT_NOT_FOUND`: Student không có trong database.
- `403 STUDENT_NOT_ACTIVE`: Student không ở trạng thái `ACTIVE`.

Endpoint chỉ đọc `students` và không có side effect.
