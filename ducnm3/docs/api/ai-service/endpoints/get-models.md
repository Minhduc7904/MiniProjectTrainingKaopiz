# `GET /ai/api/ai/models` — Liệt kê AI model

Business flow: [`get-models.md`](../../../business-flows/ai/get-models.md).

## Mục đích

Admin xem danh sách model theo trang, lọc theo provider hoặc trạng thái. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `GET /api/ai/models`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Tham số truy vấn

| Tham số | Kiểu | Quy tắc |
| --- | --- | --- |
| `providerId` | UUID | Tùy chọn; không được là UUID rỗng. |
| `status` | string | Tùy chọn; `ACTIVE` hoặc `INACTIVE`. |
| `search` | string | Tùy chọn; tìm một phần trong `modelName` hoặc `displayName`. |
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
      "id": "7d9e1b24-8a3c-4f50-b6e7-1c2d3e4f5a6b",
      "providerId": "3f2a8c1e-5b7d-4e2a-9c61-0a1b2c3d4e5f",
      "providerName": "OpenAI production",
      "providerType": "OPENAI",
      "modelName": "gpt-4o",
      "displayName": "GPT-4o",
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



## Mã trạng thái HTTP

- `200`: Thành công.
- `400`: `VALIDATION_FAILED`.
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

- Chỉ đọc `ai_models` join `ai_providers` để lấy tên và loại provider.

## Đồng bộ artifact

- Postman item: `AiService/GET Liệt kê AI model` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
