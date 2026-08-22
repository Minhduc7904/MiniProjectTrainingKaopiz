# `GET /student/api/students/summary`

Business flow: [Lấy tổng Student](../../../business-flows/students/get-students-summary.md).

## Mục đích

Student Service trả tổng số row trong bảng `students` cho dashboard quản trị.
Direct path là `GET /api/students/summary`.

## Xác thực và phân quyền

Yêu cầu `X-Actor-Type: ADMIN` và `X-Actor-Id` là UUID hợp lệ.

## Phản hồi thành công

`200 OK`, `Cache-Control: no-store`.

```json
{ "data": { "totalStudents": 125 }, "meta": { "traceId": "01J..." } }
```

## Mã trạng thái HTTP

- `200`: Đọc thành công tổng Student, không lọc status.
- `400 VALIDATION_FAILED`: Actor header không hợp lệ.
- `403 FORBIDDEN`: Actor không phải ADMIN.
- `500`: Lỗi không mong đợi khi đọc database.

Endpoint safe, idempotent, không có body và không thay đổi database.
