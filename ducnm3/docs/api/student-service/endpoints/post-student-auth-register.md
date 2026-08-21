# `POST /student/api/auth/register`

Tạo một Học viên `ACTIVE` cho demo identity không dùng JWT. Client lưu `actor`
và `id` từ response vào vùng local storage riêng của Student.

Business flow: [student-demo-auth.md](../../../business-flows/students/student-demo-auth.md).

## Request

```json
{ "email": "student@example.com", "displayName": "Student One" }
```

`email` được trim, chuyển lowercase, phải có `@` và tối đa 320 ký tự.
`displayName` bắt buộc, sau trim tối đa 200 ký tự.

## Response

`201 Created`, header `Location: /student/api/students/{id}` và response envelope:

```json
{ "data": { "actor": "STUDENT", "id": "student-uuid" }, "meta": { "traceId": "..." } }
```

`400 VALIDATION_FAILED` khi payload không hợp lệ; `409 STUDENT_EMAIL_EXISTS`
khi email đã tồn tại. Endpoint chỉ thêm một row vào `students`, không phát event.
