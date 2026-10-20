# `POST /ai/api/ai/models` — Tạo AI model

Business flow: [`post-models.md`](../../../business-flows/ai/post-models.md).

## Mục đích

Admin thêm một model cho provider để service có thể được gán. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `POST /api/ai/models`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Nội dung yêu cầu

```json
{
  "providerId": "3f2a8c1e-5b7d-4e2a-9c61-0a1b2c3d4e5f",
  "modelName": "gpt-4o",
  "displayName": "GPT-4o",
  "status": "ACTIVE"
}
```

| Field | Bắt buộc | Quy tắc |
| --- | --- | --- |
| `providerId` | Có | UUID của provider đã tồn tại. |
| `modelName` | Có | Tên model gửi cho provider (ví dụ `gpt-4o`), tối đa 100 ký tự, không chứa khoảng trắng; duy nhất trong cùng provider. |
| `displayName` | Không | Tối đa 100 ký tự; bỏ trống thì dùng `modelName`. |
| `status` | Không | `ACTIVE` hoặc `INACTIVE`, mặc định `ACTIVE`. |

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
201 Created
```

Header `Location` trỏ tới `/ai/api/ai/models/{modelId}`.

```json
{
  "data": {
    "id": "7d9e1b24-8a3c-4f50-b6e7-1c2d3e4f5a6b",
    "providerId": "3f2a8c1e-5b7d-4e2a-9c61-0a1b2c3d4e5f",
    "providerName": "OpenAI production",
    "providerType": "OPENAI",
    "modelName": "gpt-4o",
    "displayName": "GPT-4o",
    "status": "ACTIVE",
    "createdAtUtc": "2026-10-20T09:30:00Z",
    "updatedAtUtc": "2026-10-20T09:30:00Z"
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```



## Mã trạng thái HTTP

- `201`: Tạo thành công.
- `400`: `VALIDATION_FAILED`.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_PROVIDER_NOT_FOUND`.
- `409`: `AI_MODEL_CONFLICT`: provider đã có model cùng `modelName`.

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "AI_MODEL_CONFLICT",
    "message": "…",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Ghi 1 dòng `ai_models` (FK `provider_id` → `ai_providers`).

## Đồng bộ artifact

- Postman item: `AiService/POST Tạo AI model` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
