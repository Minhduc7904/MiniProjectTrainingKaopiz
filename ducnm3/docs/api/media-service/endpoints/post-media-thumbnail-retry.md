# `POST /media/api/media/{mediaId}/thumbnail/retry`

Đưa một derivation job `FAILED` trở lại RabbitMQ bằng Transactional Outbox.
Media gốc phải thuộc actor gửi request.

```json
{
  "requestedByType": "STUDENT",
  "requestedBy": "11111111-1111-1111-1111-111111111111"
}
```

Thành công trả `202 Accepted`, `Location` trỏ tới endpoint trạng thái và
`data.status=QUEUED`. Endpoint tái sử dụng cùng `jobId`,
`thumbnailMediaId` và object location nên không tạo bản ghi hoặc object trùng.

- `404 MEDIA_THUMBNAIL_NOT_FOUND`: không có job hoặc actor không sở hữu media.
- `409 MEDIA_THUMBNAIL_RETRY_CONFLICT`: job không ở trạng thái `FAILED`.
- `503`: không thể xác minh actor hoặc dependency không khả dụng.

`requestedBy*` là định danh tạm thời cho tới khi JWT cung cấp actor claims.
