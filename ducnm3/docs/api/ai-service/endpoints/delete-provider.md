# `DELETE /ai/api/ai/providers/{providerId}` — Xóa AI provider

Business flow: [`delete-provider.md`](../../../business-flows/ai/delete-provider.md).

## Mục đích

Admin xóa provider không còn model nào. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `DELETE /api/ai/providers/{providerId}`.

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
204 No Content
```

Không có body.

Response của provider chỉ có `apiKeyHint` (`****` + 4 ký tự cuối); API key và ciphertext không bao giờ được trả về.

## Mã trạng thái HTTP

- `204`: Xóa thành công, không có body.
- `400`: `VALIDATION_FAILED` khi `providerId` sai.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_PROVIDER_NOT_FOUND`.
- `409`: `AI_PROVIDER_IN_USE`: provider còn model (FK `RESTRICT`).

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "AI_PROVIDER_IN_USE",
    "message": "…",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Xóa 1 dòng `ai_providers`. Muốn xóa provider phải xóa model của nó trước.

## Đồng bộ artifact

- Postman item: `AiService/DELETE Xóa AI provider` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
