# `GET /student/api/students/{studentId}`

## Mục đích

Trả thông tin tối thiểu của một Học viên từ Student Service. Media Service dùng
endpoint này qua HTTP nội bộ để xác minh `uploadedBy`, `createdBy` và owner của
avatar; đây không phải truy vấn chéo database.

Đường dẫn công khai qua Gateway là
`GET /student/api/students/{studentId}`. Đường dẫn trực tiếp và đường dẫn
service-to-service tại Student Service là `GET /api/students/{studentId}`.

Business flow:
[`get-student-by-id.md`](../../../business-flows/students/get-student-by-id.md).
Postman: `StudentService/GET Student by ID`.

## Xác thực và phân quyền

- Xác thực: chưa bắt buộc trong phiên bản hiện tại.
- Endpoint hiện không nhận actor field và không thực hiện kiểm tra quyền sở hữu.
  Việc Media Service nhận actor từ request là giải pháp tạm thời ở phía Media;
  khi có JWT, identity sẽ được ánh xạ từ claim tại ranh giới API.

## Yêu cầu

### Tham số đường dẫn

| Tham số | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `studentId` | UUID | Có | UUID hợp lệ, khác rỗng. |

Không có query parameter hoặc request body.

## Phản hồi thành công

Phản hồi dùng [response envelope chuẩn](../../shared/response-format.md).

```http
200 OK
```

```json
{
  "data": {
    "id": "11111111-1111-1111-1111-111111111111",
    "email": "student@example.com",
    "displayName": "Student One",
    "status": "ACTIVE"
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Mã trạng thái HTTP

- `200`: Học viên tồn tại.
- `400 VALIDATION_FAILED`: `studentId` không phải UUID hợp lệ hoặc là UUID rỗng.
- `404 STUDENT_NOT_FOUND`: không có Học viên tương ứng.
- `503 SERVICE_UNAVAILABLE`: Gateway không kết nối được Student Service.
- `500 UNEXPECTED_ERROR`: lỗi database hoặc lỗi nội bộ không dự kiến.
- Lỗi dùng [error envelope chuẩn](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

- Chỉ đọc bảng `students`; không thay đổi database và không tạo tác vụ nền.
- Media Service gọi endpoint trực tiếp bằng typed HTTP client, parse response
  envelope chuẩn và ánh xạ lỗi dependency tại ranh giới Infrastructure.
- Request `GET` là idempotent và có thể được retry theo HTTP query policy dùng
  chung; `X-Correlation-Id` được truyền giữa các service.
