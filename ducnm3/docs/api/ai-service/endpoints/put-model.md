# `PUT /ai/api/ai/models/{modelId}` — Cập nhật AI model

Business flow: [`put-model.md`](../../../business-flows/ai/put-model.md).

## Mục đích

Admin sửa tên model, tên hiển thị, trạng thái. Không đổi được provider của model. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `PUT /api/ai/models/{modelId}`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Tham số đường dẫn

- `modelId` (UUID, bắt buộc, khác UUID rỗng)

### Nội dung yêu cầu

```json
{
  "modelName": "gpt-4o",
  "displayName": "GPT-4o (mặc định)",
  "status": "ACTIVE"
}
```

| Field | Bắt buộc | Quy tắc |
| --- | --- | --- |
| `modelName` | Có | Giống khi tạo; không được trùng model khác của cùng provider. |
| `displayName` | Không | Tối đa 100 ký tự; bỏ trống thì dùng `modelName`. |
| `status` | Có | `ACTIVE` hoặc `INACTIVE`. |

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
200 OK
```

```json
{
  "data": {
    "id": "7d9e1b24-8a3c-4f50-b6e7-1c2d3e4f5a6b",
    "providerId": "3f2a8c1e-5b7d-4e2a-9c61-0a1b2c3d4e5f",
    "providerName": "OpenAI production",
    "providerType": "OPENAI",
    "modelName": "gpt-4o",
    "displayName": "GPT-4o (mặc định)",
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

- `200`: Cập nhật thành công.
- `400`: `VALIDATION_FAILED`.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_MODEL_NOT_FOUND`.
- `409`: `AI_MODEL_CONFLICT`.

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

- Cập nhật `ai_models`; binding trỏ tới model này giữ nguyên.

## Đồng bộ artifact

- Postman item: `AiService/PUT Cập nhật AI model` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
