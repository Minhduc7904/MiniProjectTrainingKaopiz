# `GET /media/api/media/summary`

Business flow: [Lấy tổng Media](../../../business-flows/media/get-media-summary.md).

## Mục đích

Media Service trả tổng số row `media_objects` cho dashboard quản trị. Direct path
là `GET /api/media/summary`.

## Xác thực và phân quyền

Yêu cầu `X-Actor-Type: ADMIN` và `X-Actor-Id` là UUID hợp lệ.

## Phản hồi thành công

`200 OK`, `Cache-Control: no-store`.

```json
{ "data": { "totalMedia": 318 }, "meta": { "traceId": "01J..." } }
```

## Mã trạng thái HTTP

- `200`: Đọc thành công tổng media object, gồm mọi status.
- `400 VALIDATION_FAILED`: Actor header không hợp lệ.
- `403 FORBIDDEN`: Actor không phải ADMIN.
- `500`: Lỗi không mong đợi khi đọc database.

Endpoint chỉ đọc database Media Service; không gọi storage, không publish message và không tạo job.
