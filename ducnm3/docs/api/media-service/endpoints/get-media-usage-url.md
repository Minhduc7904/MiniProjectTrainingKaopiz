# `GET /media/api/media/usages/{usageId}/url`

Business flow: [`get-media-usage-url.md`](../../../business-flows/media/get-media-usage-url.md).

## Mục đích

Media Service trả URL phân phối của một usage ảnh active. Direct service path là
`GET /api/media/usages/{usageId}/url`; client không nhận bucket hoặc object key.

## Xác thực và phân quyền

- Xác thực và phân quyền: chưa triển khai.
- Khi JWT được thêm, Media Service phải kiểm tra quyền đọc owner trước khi cấp URL.

## Yêu cầu

| Tham số | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `usageId` | UUID | Có | UUID hợp lệ, khác rỗng; usage và media ảnh phải active/`READY`. |

Không có request body hoặc query parameter.

## Phản hồi thành công

```json
{
  "data": {
    "usageId": "62e4f6d2-2af0-4e57-a0d5-8b6ed0edc9f6",
    "mediaId": "7ba4e50e-927a-41a7-aac9-11947646a84f",
    "url": "/media/api/media/7ba4e50e-927a-41a7-aac9-11947646a84f/content",
    "expiresAtUtc": null,
    "displayOrder": 0
  },
  "meta": { "traceId": "01J..." }
}
```

`url` hiện là Gateway content URL nên `expiresAtUtc` là `null`. Một implementation presigned URL có thể đặt URL tuyệt đối và expiry mà không đổi API schema. Cache response JSON: `Cache-Control: no-store`.

## Mã trạng thái HTTP

- `200`: usage ảnh active có URL phân phối.
- `400 INVALID_MEDIA`: `usageId` không hợp lệ.
- `404 MEDIA_USAGE_NOT_FOUND`: usage không tồn tại, đã soft-delete hoặc media không còn `READY`.

Lỗi dùng [error envelope chuẩn](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

- Request safe, idempotent, chỉ đọc `media_usages` và `media_objects`.
- Chỉ cấp URL cho `IMAGE` có media `READY`, chưa soft-delete.
- Không thay đổi database, không gọi MinIO, không publish message.
