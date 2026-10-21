# `PUT /ai/api/ai/service-bindings/{serviceName}` — Gán (hoặc đổi) model AI cho service

Business flow: [`put-service-binding.md`](../../../business-flows/ai/put-service-binding.md).

## Mục đích

Admin chọn model cho một service. Idempotent: gọi lại với cùng model không đổi kết quả. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `PUT /api/ai/service-bindings/{serviceName}`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Tham số đường dẫn

- `serviceName` (string, bắt buộc): một trong `course-service`, `student-service`, `media-service`, `notification-service`, `scheduler-service`, `admin-service`, `document-extraction`, `lesson-synthesis`, `quiz-authoring`. Ba khóa cuối là khóa tác vụ AI của tính năng soạn nội dung Lesson (xem [`post-completions.md`](post-completions.md)). `ai-service` và gateway không được gán.

### Nội dung yêu cầu

```json
{
  "modelId": "7d9e1b24-8a3c-4f50-b6e7-1c2d3e4f5a6b"
}
```

| Field | Bắt buộc | Quy tắc |
| --- | --- | --- |
| `modelId` | Có | UUID của model đang `ACTIVE` thuộc provider đang `ACTIVE`. |

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
201 Created (tạo mới) hoặc 200 OK (thay model)
```

```json
{
  "data": {
    "id": "b1c2d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e",
    "serviceName": "course-service",
    "modelId": "7d9e1b24-8a3c-4f50-b6e7-1c2d3e4f5a6b",
    "modelName": "gpt-4o",
    "modelDisplayName": "GPT-4o",
    "modelStatus": "ACTIVE",
    "providerId": "3f2a8c1e-5b7d-4e2a-9c61-0a1b2c3d4e5f",
    "providerName": "OpenAI production",
    "providerType": "OPENAI",
    "providerStatus": "ACTIVE",
    "createdAtUtc": "2026-10-20T09:30:00Z",
    "updatedAtUtc": "2026-10-20T09:30:00Z"
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```



## Mã trạng thái HTTP

- `201`: Service chưa có binding, đã tạo mới.
- `200`: Service đã có binding, đã thay model.
- `400`: `VALIDATION_FAILED`: `serviceName` không được hỗ trợ hoặc `modelId` sai.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_MODEL_NOT_FOUND` hoặc `AI_PROVIDER_NOT_FOUND`.
- `409`: `AI_REFERENCE_INACTIVE`: model hoặc provider đang `INACTIVE`.

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "AI_REFERENCE_INACTIVE",
    "message": "…",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Upsert `ai_service_bindings` theo unique `service_name` (mỗi service một binding).
- Phase 2: worker sẽ tra binding này để biết gọi model nào.

## Đồng bộ artifact

- Postman item: `AiService/PUT Gán (hoặc đổi) model AI cho service` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
