# `GET /student/api/auth/me`

Xác minh Student identity demo đang được client lưu trước khi vào trang học.
Endpoint không dùng JWT; actor được gửi tại boundary bằng `X-Actor-Type: STUDENT`
và `X-Actor-Id: {student-uuid}`.

Business flow: [student-demo-auth.md](../../../business-flows/students/student-demo-auth.md).

## Response

Khi header hợp lệ, Student tồn tại và `ACTIVE`, API trả `200 OK`,
`Cache-Control: no-store`:

```json
{
  "data": {
    "id": "student-uuid",
    "email": "student@example.com",
    "displayName": "Student One",
    "status": "ACTIVE"
  },
  "meta": { "traceId": "..." }
}
```

- `400 VALIDATION_FAILED`: actor header thiếu/sai UUID.
- `403 FORBIDDEN`: actor không phải Student; `403 STUDENT_NOT_ACTIVE`: Student bị khóa/inactive.
- `404 STUDENT_NOT_FOUND`: ID trong header không tồn tại.

Endpoint chỉ đọc database, không thay đổi session server-side.
