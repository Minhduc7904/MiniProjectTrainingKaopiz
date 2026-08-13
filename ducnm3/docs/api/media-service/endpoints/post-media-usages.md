# `POST /media/api/media/usages`

## Mục đích

Đăng ký một media `READY` làm avatar của Học viên. Media Service sở hữu liên kết
usage và bảo đảm mỗi Học viên chỉ có một avatar active.

Đường dẫn công khai qua Gateway là `POST /media/api/media/usages`; đường dẫn
trực tiếp tại Media Service là `POST /api/media/usages`.

## Xác thực và phân quyền

- Xác thực: chưa bắt buộc trong phiên bản hiện tại.
- `createdByType` và `createdBy` là định danh request tạm thời trong giai đoạn
  chưa có xác thực. Hiện chỉ hỗ trợ actor type `STUDENT`, và UUID actor phải tồn
  tại trong Student Service.
- Actor tạo usage khác khái niệm với owner. `createdByType=STUDENT` phân loại
  tác nhân, còn `ownerType=STUDENT_AVATAR` phân loại tài nguyên/vị trí sở hữu;
  `createdBy` không mặc nhiên bằng `ownerId`.
- Khi JWT được triển khai, actor type/ID sẽ được ánh xạ từ claim thay vì nhận từ
  JSON request.

## Yêu cầu

Content type bắt buộc là `application/json`.

```json
{
  "mediaId": "8c2bf508-60bb-44d4-91aa-1baad98db09c",
  "ownerService": "STUDENT",
  "ownerType": "STUDENT_AVATAR",
  "ownerId": "11111111-1111-1111-1111-111111111111",
  "usageType": "AVATAR",
  "displayOrder": 0,
  "createdByType": "STUDENT",
  "createdBy": "11111111-1111-1111-1111-111111111111"
}
```

| Field | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `mediaId` | UUID | Có | Media phải tồn tại, chưa bị xóa và có trạng thái `READY`. |
| `ownerService` | string | Có | Phiên bản hiện tại chỉ nhận `STUDENT`. |
| `ownerType` | string | Có | Phiên bản hiện tại chỉ nhận `STUDENT_AVATAR`. |
| `ownerId` | UUID | Có | ID Học viên sở hữu avatar; phải tồn tại nếu khác actor đang tạo usage. |
| `usageType` | string | Có | Phiên bản hiện tại chỉ nhận `AVATAR`. |
| `displayOrder` | uint | Có | Số nguyên không âm; với avatar thường dùng `0`. |
| `createdByType` | string | Có | Actor type tạo usage; hiện chỉ hỗ trợ `STUDENT`. |
| `createdBy` | UUID | Có | ID actor tạo usage; phải tồn tại trong Student Service. |

Tổ hợp duy nhất được hỗ trợ hiện tại là
`STUDENT/STUDENT_AVATAR/AVATAR`. Các giá trị enum không phân biệt hoa/thường ở
tầng Application.

## Phản hồi thành công

Phản hồi dùng [response envelope chuẩn](../../shared/response-format.md).

```http
201 Created
Location: /api/media/usages/555b1076-2cb1-4211-9207-c2ae685b9e06
```

```json
{
  "data": {
    "id": "555b1076-2cb1-4211-9207-c2ae685b9e06",
    "mediaId": "8c2bf508-60bb-44d4-91aa-1baad98db09c",
    "ownerService": "STUDENT",
    "ownerType": "STUDENT_AVATAR",
    "ownerId": "11111111-1111-1111-1111-111111111111",
    "usageType": "AVATAR",
    "displayOrder": 0,
    "createdAtUtc": "2026-08-13T07:30:00Z"
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Mã trạng thái HTTP

- `201`: usage mới được tạo và trở thành avatar active.
- `400 INVALID_MEDIA`: UUID không hợp lệ hoặc tổ hợp
  `ownerService/ownerType/usageType` chưa được hỗ trợ.
- `400 INVALID_ACTOR_TYPE`: actor type thiếu hoặc chưa được hỗ trợ.
- `404 ACTOR_NOT_FOUND`: `createdBy` không tồn tại trong Student Service.
- `404 OWNER_NOT_FOUND`: `ownerId` khác actor và Học viên sở hữu không tồn tại.
- `404 MEDIA_NOT_FOUND`: `mediaId` không tồn tại.
- `409 MEDIA_NOT_READY`: media còn `PENDING`, đã `FAILED` hoặc đã bị xóa mềm.
- `409 MEDIA_USAGE_CONFLICT`: ràng buộc duy nhất của usage active bị xung đột
  khi xử lý đồng thời.
- `503 STUDENT_SERVICE_UNAVAILABLE`: Student Service không khả dụng khi Media
  Service xác minh actor hoặc owner.
- `503 SERVICE_UNAVAILABLE`: Gateway không kết nối được Media Service.
- `500 UNEXPECTED_ERROR`: lỗi database hoặc lỗi nội bộ không dự kiến.
- Lỗi dùng [error envelope chuẩn](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

- Media Service xác minh actor trước, sau đó xác minh owner khi actor không phải
  chính Học viên sở hữu.
- Việc thay avatar chạy trong transaction `SERIALIZABLE`: mọi
  `STUDENT/STUDENT_AVATAR/AVATAR` active trước đó của cùng `ownerId` được xóa
  mềm, rồi usage mới được tạo.
- Generated column `active_student_avatar_owner_id` cùng unique index bảo vệ
  quy tắc tối đa một avatar active cho mỗi Học viên, kể cả khi có request đồng
  thời.
- `createdByType/createdBy` được lưu riêng với
  `ownerService/ownerType/ownerId`; hai nhóm field không được dùng thay thế nhau.
- Request không có idempotency key. Gửi lại sẽ thay usage active; xung đột đồng
  thời được trả bằng `409 MEDIA_USAGE_CONFLICT`.
