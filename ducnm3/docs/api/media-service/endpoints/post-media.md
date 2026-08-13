# `POST /media/api/media`

## Mục đích

Tải một tệp lên Media Service, lưu metadata trong database do service sở hữu và
lưu dữ liệu nhị phân trong MinIO. Đường dẫn công khai qua Gateway là
`POST /media/api/media`; đường dẫn trực tiếp tại Media Service là
`POST /api/media`.

Business flow:
[`post-media.md`](../../../business-flows/media/post-media.md).
Postman: `MediaService/POST Upload media`.

## Xác thực và phân quyền

- Xác thực: chưa bắt buộc trong phiên bản hiện tại.
- `uploadedByType` và `uploadedBy` là định danh request tạm thời trong giai đoạn
  chưa có xác thực. Hiện chỉ hỗ trợ actor type `STUDENT`, và UUID actor phải tồn
  tại trong Student Service.
- Hai field này không phải dữ liệu tin cậy do hệ thống xác thực cấp. Khi JWT được
  triển khai, Media Service sẽ lấy actor type/ID từ claim thay vì nhận chúng từ
  multipart form.

## Yêu cầu

Content type bắt buộc là `multipart/form-data`.

| Field | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `file` | file | Có | Không rỗng, có tên và phần mở rộng; MIME phải khớp `mediaType`; kích thước không vượt giới hạn cấu hình. |
| `mediaType` | string | Có | `IMAGE`, `VIDEO`, `DOCUMENT`, `AUDIO` hoặc `OTHER`; không phân biệt hoa/thường và được chuẩn hóa thành chữ hoa. |
| `uploadedByType` | string | Có | Actor type của người thực hiện request; hiện chỉ hỗ trợ `STUDENT`. |
| `uploadedBy` | UUID | Có | ID actor thực hiện upload; phải là UUID khác rỗng và tồn tại trong Student Service. |

Ví dụ:

```bash
curl -X POST http://localhost:5100/media/api/media \
  -F "file=@avatar.png;type=image/png" \
  -F "mediaType=IMAGE" \
  -F "uploadedByType=STUDENT" \
  -F "uploadedBy=11111111-1111-1111-1111-111111111111"
```

Giới hạn mặc định là 10 MiB cho `IMAGE`, 500 MiB cho `VIDEO`, 50 MiB cho
`DOCUMENT`, 100 MiB cho `AUDIO`, 25 MiB cho `OTHER`; toàn bộ multipart request
không vượt 525 MiB. Môi trường triển khai có thể thay đổi các giới hạn này.

## Phản hồi thành công

Phản hồi dùng [response envelope chuẩn](../../shared/response-format.md).

```http
201 Created
Location: /api/media/8c2bf508-60bb-44d4-91aa-1baad98db09c
```

```json
{
  "data": {
    "id": "8c2bf508-60bb-44d4-91aa-1baad98db09c",
    "mediaType": "IMAGE",
    "contentType": "image/png",
    "sizeBytes": 24576,
    "status": "READY",
    "contentUrl": "/media/api/media/8c2bf508-60bb-44d4-91aa-1baad98db09c/content"
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

`Location` và `data.contentUrl` là public Gateway path có tiền tố `/media`.

## Mã trạng thái HTTP

- `201`: upload hoàn tất, checksum đã được lưu và trạng thái là `READY`.
- `400 INVALID_MEDIA`: thiếu/sai multipart field, file rỗng, tên file không có
  phần mở rộng, `mediaType` không hỗ trợ hoặc UUID không hợp lệ.
- `400 INVALID_ACTOR_TYPE`: actor type thiếu hoặc chưa được hỗ trợ.
- `404 ACTOR_NOT_FOUND`: `uploadedBy` không tồn tại trong Student Service.
- `413 PAYLOAD_TOO_LARGE`: file hoặc toàn bộ multipart request vượt giới hạn
  được cấu hình.
- `415 UNSUPPORTED_MEDIA_TYPE`: MIME của file không khớp `mediaType`.
- `503 STUDENT_SERVICE_UNAVAILABLE`: không thể xác minh actor với Student Service.
- `503 MEDIA_UPLOAD_FAILED`: MinIO không nhận được object hoặc không tạo được
  checksum SHA-256.
- `503 SERVICE_UNAVAILABLE`: Gateway không kết nối được Media Service.
- `500 UNEXPECTED_ERROR`: lỗi nội bộ không dự kiến, gồm lỗi hoàn tất database;
  service vẫn thử compensation trước khi middleware trả lỗi an toàn.
- Lỗi dùng [error envelope chuẩn](../../shared/error-format.md); không trả
  credential, bucket, object key hoặc `failure_reason`.

## Điều kiện nghiệp vụ và tác động phụ

1. Media Service xác minh actor, cấp phát bucket/object key nội bộ và ghi
   `media_objects` với trạng thái `PENDING` trước khi gọi MinIO.
2. MinIO nhận stream và tính checksum SHA-256 trong lúc upload.
3. Khi thành công, database được cập nhật `READY`, `checksum_sha256` và
   `completed_at`; chỉ lúc đó API mới trả `201`.
4. Khi upload bị hủy/lỗi, thiếu checksum hoặc bước hoàn tất database lỗi, service
   cố gắng xóa object để compensation và đánh dấu bản ghi `FAILED`.
5. Compensation là best effort. Việc xử lý các bản ghi `PENDING` bị bỏ lại do
   process dừng đột ngột được hoãn cho tác vụ cleanup của Scheduler; endpoint
   hiện tại không tự quét các bản ghi stale.

Request không idempotent: gửi lại cùng file tạo một `mediaId` và object mới.
