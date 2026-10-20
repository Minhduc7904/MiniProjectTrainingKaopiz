# `GET /ai/api/ai/service-bindings/{serviceName}` — Xem AI đã gán cho một service

Business flow: [`get-service-binding.md`](../../../business-flows/ai/get-service-binding.md).

## Mục đích

Admin xem model AI của một service. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `GET /api/ai/service-bindings/{serviceName}`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Tham số đường dẫn

- `serviceName` (string, bắt buộc): một trong `course-service`, `student-service`, `media-service`, `notification-service`, `scheduler-service`, `admin-service`. `ai-service` và gateway không được gán.

Không có nội dung yêu cầu (body).

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
200 OK
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

- `200`: Thành công.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_SERVICE_BINDING_NOT_FOUND`.

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "AI_SERVICE_BINDING_NOT_FOUND",
    "message": "…",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Chỉ đọc.

## Đồng bộ artifact

- Postman item: `AiService/GET Xem AI đã gán cho một service` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
