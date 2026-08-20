# `GET /media/api/media/usages/urls`

Business flow: [`get-media-usage-urls.md`](../../../business-flows/media/get-media-usage-urls.md).

## Mục đích

Trả URL phân phối cho mọi usage media active của đúng một owner. Direct service
path là `GET /api/media/usages/urls`.

## Xác thực và phân quyền

- Xác thực và phân quyền: chưa triển khai.
- Khi có JWT, Media Service phải chỉ trả owner mà caller có quyền đọc.

## Yêu cầu

| Query | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `ownerService` | string | Có | Tên service owner, không rỗng; được chuẩn hóa uppercase. |
| `ownerType` | string | Có | Loại owner, không rỗng; được chuẩn hóa uppercase. |
| `usageType` | string | Có | Loại usage, không rỗng; được chuẩn hóa uppercase. |
| `ownerId` | UUID | Có | UUID hợp lệ, khác rỗng. |

Ví dụ: `GET /media/api/media/usages/urls?ownerService=STUDENT&ownerType=STUDENT_AVATAR&usageType=AVATAR&ownerId=9c2f278b-01de-4f02-aaf5-099db6a51491`.

## Phản hồi thành công

`200 OK` trả envelope với `data` là mảng `MediaUsageUrlResponse`. Mảng không phân trang vì đã bị giới hạn trong một owner; thứ tự ổn định là `displayOrder`, sau đó `usageId`.

```json
{
  "data": [
    {
      "usageId": "62e4f6d2-2af0-4e57-a0d5-8b6ed0edc9f6",
      "mediaId": "7ba4e50e-927a-41a7-aac9-11947646a84f",
      "url": "/media/api/media/7ba4e50e-927a-41a7-aac9-11947646a84f/content",
      "expiresAtUtc": null,
      "displayOrder": 0
    }
  ],
  "meta": { "traceId": "01J..." }
}
```

Mỗi phần tử còn có `thumbnailUrl` (nếu có), `mediaType`, `contentType` và
`originalFileName`. Mảng rỗng là kết quả hợp lệ khi owner chưa có usage media active.
Response dùng `Cache-Control: no-store`.

## Mã trạng thái HTTP

- `200`: trả toàn bộ usage media active của owner, có thể rỗng.
- `400 INVALID_MEDIA`: thiếu/sai query parameter, gồm `usageType`.

Lỗi dùng [error envelope chuẩn](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

- Request safe, idempotent, chỉ đọc `media_usages` và `media_objects`.
- Không trả media soft-delete hoặc chưa `READY`. Thumbnail chỉ được trả khi đó là ảnh `READY`.
- Không thay đổi database, không gọi MinIO, không publish message.
