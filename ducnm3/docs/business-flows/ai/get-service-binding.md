# `GET /ai/api/ai/service-bindings/{serviceName}` — Xem AI đã gán cho một service

API contract: [`get-service-binding.md`](../../api/ai-service/endpoints/get-service-binding.md)

## Mục tiêu

Admin xem model AI của một service.

## Actor và thành phần

- Admin (actor `ADMIN`).
- API Gateway (route `/ai/{**catch-all}`, bỏ tiền tố `/ai`).
- AI Service (Api → Application → Infrastructure).
- MySQL `lms_ai_db`.

## Điều kiện trước

- Header `X-Actor-Type: ADMIN` và `X-Actor-Id` hợp lệ.
- Body/query đúng contract trong tài liệu API.

## UML luồng chạy

```mermaid
sequenceDiagram
    participant Admin
    participant Gateway
    participant API as AI Service API
    participant App as Application
    participant DB as MySQL lms_ai_db

    Admin->>Gateway: GET /ai/api/ai/service-bindings/{serviceName}
    Gateway->>API: Forward request
    API->>App: Kiểm tra actor Admin, validate, xử lý
    App->>DB: Đọc/ghi ai_service_bindings
    DB-->>App: Kết quả
    App-->>API: Response model
    API-->>Gateway: HTTP 200
    Gateway-->>Admin: HTTP response
```

## Luồng chính

1. Admin gửi `GET /ai/api/ai/service-bindings/{serviceName}`.
2. Handler đọc binding theo `service_name`.
3. API trả `200` hoặc `404`.

## Luồng lỗi

- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_SERVICE_BINDING_NOT_FOUND`.

## Dữ liệu và side effects

- Chỉ đọc.

## Test mapping

- Unit: [`docs/tests/ai-service/unit.md`](../../tests/ai-service/unit.md).
- Component: [`docs/tests/ai-service/component.md`](../../tests/ai-service/component.md).
