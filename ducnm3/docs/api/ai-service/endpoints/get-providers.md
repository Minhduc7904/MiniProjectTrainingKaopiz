# `GET /ai/api/ai/providers` — Liệt kê AI provider

Business flow: [`get-providers.md`](../../../business-flows/ai/get-providers.md).

## Mục đích

Admin xem danh sách provider theo trang để quản lý cấu hình. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `GET /api/ai/providers`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Tham số truy vấn

| Tham số | Kiểu | Quy tắc |
| --- | --- | --- |
| `search` | string | Tùy chọn; trim, tìm một phần trong tên provider. |
| `status` | string | Tùy chọn; `ACTIVE` hoặc `INACTIVE`. |
| `providerType` | string | Tùy chọn; một trong các loại AI được hỗ trợ. |
| `page` | int | Mặc định `1`, tối thiểu `1`. |
| `pageSize` | int | Mặc định `20`, từ `1` đến `100`. |

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
200 OK
```

```json
{
  "data": [
    {
      "id": "3f2a8c1e-5b7d-4e2a-9c61-0a1b2c3d4e5f",
      "name": "OpenAI production",
      "providerType": "OPENAI",
      "baseUrl": "https://api.openai.com/v1",
      "apiKeyHint": "****1234",
      "status": "ACTIVE",
      "createdAtUtc": "2026-10-20T09:30:00Z",
      "updatedAtUtc": "2026-10-20T09:30:00Z"
    }
  ],
  "meta": {
    "traceId": "0HN7K2Q5V8M1A",
    "pagination": {
      "type": "offset",
      "page": 1,
      "pageSize": 20,
      "totalItems": 1,
      "totalPages": 1
    }
  }
}
```

Sắp xếp ổn định theo `created_at` giảm dần rồi `id`, phân trang offset.

Response của provider chỉ có `apiKeyHint` (`****` + 4 ký tự cuối); API key và ciphertext không bao giờ được trả về.

## Mã trạng thái HTTP

- `200`: Thành công; danh sách rỗng vẫn là `200` với `data: []`.
- `400`: `VALIDATION_FAILED` khi filter hoặc phân trang sai.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "400",
    "message": "…",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Chỉ đọc `ai_providers`; không trả API key hay ciphertext.

## Đồng bộ artifact

- Postman item: `AiService/GET Liệt kê AI provider` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
