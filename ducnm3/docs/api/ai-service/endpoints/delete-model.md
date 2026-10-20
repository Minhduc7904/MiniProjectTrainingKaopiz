# `DELETE /ai/api/ai/models/{modelId}` — Xóa AI model

Business flow: [`delete-model.md`](../../../business-flows/ai/delete-model.md).

## Mục đích

Admin xóa model chưa được gán cho service nào. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `DELETE /api/ai/models/{modelId}`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Tham số đường dẫn

- `modelId` (UUID, bắt buộc, khác UUID rỗng)

Không có nội dung yêu cầu (body).

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
204 No Content
```

Không có body.



## Mã trạng thái HTTP

- `204`: Xóa thành công.
- `400`: `VALIDATION_FAILED`.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_MODEL_NOT_FOUND`.
- `409`: `AI_MODEL_IN_USE`: model còn binding (FK `RESTRICT`).

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "AI_MODEL_IN_USE",
    "message": "…",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Xóa 1 dòng `ai_models`. Phải gỡ binding trước.

## Đồng bộ artifact

- Postman item: `AiService/DELETE Xóa AI model` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
