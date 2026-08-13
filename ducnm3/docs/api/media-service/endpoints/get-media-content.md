# Lấy nội dung media

## Mục đích

Stream trực tiếp object đã upload từ MinIO qua Media Service và API Gateway,
không trả `bucket` hoặc `objectKey` cho client.

Business flow:
[`get-media-content.md`](../../../business-flows/media/get-media-content.md).
Postman: `MediaService/GET Media content`.

## Contract

- Owner service: Media Service.
- Method: `GET`.
- Service path: `/api/media/{mediaId}/content`.
- Public Gateway path: `/media/api/media/{mediaId}/content`.
- Authentication: chưa triển khai.
- Authorization: chưa triển khai; endpoint hiện chỉ kiểm tra trạng thái media.

## Request

Path parameter:

```text
mediaId // UUID của media_objects
```

Ví dụ:

```http
GET /media/api/media/7e673b57-e0af-40ee-a141-91421c3a0101/content
```

## Success response

`200 OK` trả binary stream, không bọc trong JSON envelope.

Headers:

```text
Content-Type        // MIME đã validate khi upload
Content-Length      // Kích thước object đã lưu
Content-Disposition // inline; filename*=UTF-8''...
Cache-Control       // no-store
```

Response được stream trực tiếp, không buffer toàn bộ file trong memory.

## Validation và business preconditions

- `mediaId` phải là UUID khác rỗng.
- `media_objects` phải tồn tại và chưa soft-delete.
- Media phải có trạng thái `READY`.
- Object storage phải khả dụng.

## Status và error code

- `400 INVALID_MEDIA`: `mediaId` không hợp lệ.
- `404 MEDIA_NOT_FOUND`: media không tồn tại hoặc đã soft-delete.
- `409 MEDIA_NOT_READY`: media chưa `READY`.
- `503 STORAGE_UNAVAILABLE`: không thể đọc object từ MinIO trước khi response
  bắt đầu.

Error response trước khi binary response bắt đầu sử dụng envelope tại
[`../../shared/error-format.md`](../../shared/error-format.md).

## Giới hạn hiện tại

- Chưa có authentication/authorization.
- Chưa hỗ trợ HTTP range request, resume download hoặc presigned URL.
- `Cache-Control: no-store` được dùng cho tới khi có policy access/cache rõ ràng.
- Nếu storage lỗi sau khi đã stream một phần response, kết nối bị ngắt; không thể
  đổi response đó thành JSON error envelope.
