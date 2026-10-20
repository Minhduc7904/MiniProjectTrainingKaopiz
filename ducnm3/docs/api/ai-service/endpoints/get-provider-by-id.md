# `GET /ai/api/ai/providers/{providerId}` — Xem chi tiết AI provider

Business flow: [`get-provider-by-id.md`](../../../business-flows/ai/get-provider-by-id.md).

## Mục đích

Admin xem một provider theo ID. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `GET /api/ai/providers/{providerId}`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Tham số đường dẫn

- `providerId` (UUID, bắt buộc, khác `00000000-0000-0000-0000-000000000000`)

Không có nội dung yêu cầu (body).

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
200 OK
```

```json
{
  "data": {
    "id": "3f2a8c1e-5b7d-4e2a-9c61-0a1b2c3d4e5f",
    "name": "OpenAI production",
    "providerType": "OPENAI",
    "baseUrl": "https://api.openai.com/v1",
    "apiKeyHint": "****1234",
    "status": "ACTIVE",
    "createdAtUtc": "2026-10-20T09:30:00Z",
    "updatedAtUtc": "2026-10-20T09:30:00Z"
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

Response của provider chỉ có `apiKeyHint` (`****` + 4 ký tự cuối); API key và ciphertext không bao giờ được trả về.

## Mã trạng thái HTTP

- `200`: Thành công.
- `400`: `VALIDATION_FAILED` khi `providerId` không phải UUID hợp lệ.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_PROVIDER_NOT_FOUND`.

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "AI_PROVIDER_NOT_FOUND",
    "message": "…",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Chỉ đọc `ai_providers`; không trả API key.

## Đồng bộ artifact

- Postman item: `AiService/GET Xem chi tiết AI provider` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
