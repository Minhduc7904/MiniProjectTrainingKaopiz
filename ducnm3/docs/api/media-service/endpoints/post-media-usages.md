# `POST /media/api/media/usages`

## Mục đích

Đăng ký một media `READY` cho avatar Học viên hoặc các mục Course được hỗ trợ.
Media Service sở hữu liên kết usage và soft-delete usage cũ khi thay thế.

Đường dẫn công khai qua Gateway là `POST /media/api/media/usages`; đường dẫn
trực tiếp tại Media Service là `POST /api/media/usages`.

Business flow:
[`post-media-usages.md`](../../../business-flows/media/post-media-usages.md).
Postman: `MediaService/POST Create media usage`.

## Xác thực và phân quyền

- Xác thực: chưa bắt buộc trong phiên bản hiện tại.
- `createdByType` và `createdBy` là định danh request tạm thời trong giai đoạn
  chưa có xác thực. Hỗ trợ actor type `STUDENT` và `ADMIN`; UUID actor phải được
  actor validator tương ứng xác minh.
- Actor tạo usage khác khái niệm với owner. `createdByType` phân loại
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
  "displayOrder": 0
}
```

Request gửi actor qua header `X-Actor-Type` và `X-Actor-Id`.

| Field | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `mediaId` | UUID | Có | Media original phải tồn tại, chưa bị xóa và có trạng thái `READY`. |
| `ownerService` | string | Có | `STUDENT` hoặc `COURSE`; phải khớp tuple usage. |
| `ownerType` | string | Có | `STUDENT_AVATAR`, `COURSE_THUMBNAIL`, `COURSE_GALLERY` hoặc `LESSON_ATTACHMENT`. |
| `ownerId` | UUID | Có | ID owner; Course/Lesson ID không được Media Service lookup trong phiên bản này. |
| `usageType` | string | Có | `AVATAR`, `THUMBNAIL` hoặc `ATTACHMENT`, khớp tuple owner. |
| `displayOrder` | uint | Có | Số nguyên không âm; với avatar thường dùng `0`. |
| `X-Actor-Type` | header string | Có | `STUDENT` hoặc `ADMIN`, tùy policy của tuple. |
| `X-Actor-Id` | header UUID | Có | ID actor tạo usage; được actor validator xác minh. |

Student chỉ gán `STUDENT/STUDENT_AVATAR/AVATAR` cho chính mình và media phải là
ảnh nguồn `READY`. Admin tạo các tuple `COURSE/*`; `COURSE_THUMBNAIL` và
`COURSE_GALLERY` dùng ảnh nguồn `READY`, còn `LESSON_ATTACHMENT` nhận mọi media
nguồn `READY` hợp lệ theo allowlist upload. Attachment không dùng chung owner
với media tham chiếu trong Markdown (`LESSON_CONTENT`).

Mọi direct usage phải trỏ tới media original. `MEDIA/MEDIA_THUMBNAIL/THUMBNAIL`
là usage nội bộ chỉ do Media Worker tạo sau derivation; thumbnail WebP phải trỏ
đúng media original qua `ownerId=sourceMediaId` và không được gửi qua API này.
`COURSE_THUMBNAIL` lưu ảnh nguồn để Course detail hiển thị rõ; list Course có
thể dùng thumbnail WebP dẫn xuất của ảnh nguồn mà không tạo usage mới.

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

- `201`: usage mới được tạo và trở thành avatar hoặc Course thumbnail active.
- `400 INVALID_MEDIA`: UUID không hợp lệ hoặc tổ hợp
  `ownerService/ownerType/usageType` chưa được hỗ trợ.
- `400 INVALID_ACTOR_TYPE`: actor type thiếu hoặc chưa được hỗ trợ.
- `404 ACTOR_NOT_FOUND`: `createdBy` không tồn tại trong actor service tương ứng.
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

- Media Service xác minh actor trước. Student phải là owner avatar; Course usage
  chỉ kiểm tra actor là Admin, không gọi Course Service để lookup owner.
- Việc thay avatar chạy trong transaction `SERIALIZABLE`: mọi
  `STUDENT/STUDENT_AVATAR/AVATAR` active trước đó của cùng `ownerId` được xóa
  mềm, rồi usage mới được tạo.
- Generated column `active_student_avatar_owner_id` cùng unique index bảo vệ
  quy tắc tối đa một avatar active cho mỗi Học viên, kể cả khi có request đồng
  thời.
- Media Worker tạo `MEDIA/MEDIA_THUMBNAIL/THUMBNAIL` qua internal repository
  method riêng. Generated column `active_media_thumbnail_owner_id` bảo đảm mỗi
  media gốc chỉ có một thumbnail active; client không thể gọi direct API để
  tạo hoặc thay thế usage này.
- `createdByType/createdBy` được lưu riêng với
  `ownerService/ownerType/ownerId`; hai nhóm field không được dùng thay thế nhau.
- Request không có idempotency key. Gửi lại sẽ thay usage active; xung đột đồng
  thời được trả bằng `409 MEDIA_USAGE_CONFLICT`.
