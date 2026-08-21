# `GET /media/api/media/library`

## Mục đích

Liệt kê media **original** của actor hiện tại để người dùng chọn và gán usage.
Đường dẫn trực tiếp tại Media Service là `GET /api/media/library`.

Business flow: [`get-media-library.md`](../../../business-flows/media/get-media-library.md).

## Xác thực và visibility

- Actor được nhận qua `X-Actor-Type` và `X-Actor-Id`.
- Chỉ trả media do đúng actor upload, chưa soft-delete và có
  `sourceMediaId = null`. Thumbnail/derivative không xuất hiện như một item
  có thể chọn.

## Query

| Query | Kiểu | Mặc định | Quy tắc |
| --- | --- | --- | --- |
| `mediaType` | string | Không có | Một trong `IMAGE`, `VIDEO`, `DOCUMENT`, `AUDIO`, `OTHER`; không phân biệt hoa thường. |
| `pageSize` | integer | `20` | Từ `1` đến `50`. |
| `cursor` | string | Không có | Cursor opaque của item cuối trang trước. |

Không có request body. Kết quả có total order `createdAtUtc DESC, id DESC`.

## Phản hồi thành công

`200 OK`

```json
{
  "data": {
    "items": [
      {
        "id": "8c2bf508-60bb-44d4-91aa-1baad98db09c",
        "mediaType": "IMAGE",
        "contentUrl": "/media/api/media/8c2bf508-60bb-44d4-91aa-1baad98db09c/content",
        "thumbnailMediaId": "7b920767-6924-42b1-889f-5e59d3e8f69f",
        "thumbnailUrl": "/media/api/media/7b920767-6924-42b1-889f-5e59d3e8f69f/content",
        "thumbnailStatus": "READY"
      }
    ],
    "nextCursor": null,
    "hasMore": false
  },
  "meta": { "traceId": "01J..." }
}
```

`id` và `contentUrl` luôn thuộc media original và là giá trị client phải dùng
khi gán usage hoặc chèn Markdown. `thumbnailMediaId`/`thumbnailUrl` chỉ dùng để
render preview trong Media Library; không được gửi làm `mediaId` cho usage.

## Mã trạng thái và side effects

- `200`: kể cả khi không có media phù hợp (`items: []`).
- `400 INVALID_MEDIA`: `mediaType`, `pageSize` hoặc `cursor` không hợp lệ.

Endpoint safe, idempotent và chỉ đọc; response không cache để phản ánh trạng
thái upload/derivation mới nhất.
