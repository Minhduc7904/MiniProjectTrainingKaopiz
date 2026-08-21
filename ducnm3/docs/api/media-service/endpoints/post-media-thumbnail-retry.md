# `POST /media/api/media/{mediaId}/thumbnail/retry`

Đưa một derivation job `FAILED` trở lại RabbitMQ bằng Transactional Outbox.
Media gốc phải thuộc actor gửi request.

Không có request body. Actor được đọc từ `X-Actor-Type` (`ADMIN` hoặc `STUDENT`)
và `X-Actor-Id` UUID hợp lệ.

Thành công trả `202 Accepted`, `Location` trỏ tới endpoint trạng thái và
`data.status=QUEUED`. Endpoint tái sử dụng cùng `jobId`,
`thumbnailMediaId` và object location nên không tạo bản ghi hoặc object trùng.

- `404 MEDIA_THUMBNAIL_NOT_FOUND`: không có job hoặc actor không sở hữu media.
- `409 MEDIA_THUMBNAIL_RETRY_CONFLICT`: job không ở trạng thái `FAILED`.
- `503`: không thể xác minh actor hoặc dependency không khả dụng.

Actor header là định danh demo tạm thời cho tới khi JWT cung cấp actor claims.
