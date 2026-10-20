# `GET /ai/api/ai/models` — Liệt kê AI model

API contract: [`get-models.md`](../../api/ai-service/endpoints/get-models.md)

## Mục tiêu

Admin xem danh sách model theo trang, lọc theo provider hoặc trạng thái.

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

    Admin->>Gateway: GET /ai/api/ai/models
    Gateway->>API: Forward request
    API->>App: Kiểm tra actor Admin, validate, xử lý
    App->>DB: Đọc/ghi ai_models
    DB-->>App: Kết quả
    App-->>API: Response model
    API-->>Gateway: HTTP 200
    Gateway-->>Admin: HTTP response
```

## Luồng chính

1. Admin gửi `GET /ai/api/ai/models` với filter.
2. `GetAiModelsQuery.Create` normalize filter và phân trang.
3. Repository đếm và đọc trang.
4. API trả `200` cùng `meta.pagination` offset.

## Luồng lỗi

- `400`: `VALIDATION_FAILED`.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).

## Dữ liệu và side effects

- Chỉ đọc `ai_models` join `ai_providers` để lấy tên và loại provider.

## Test mapping

- Unit: [`docs/tests/ai-service/unit.md`](../../tests/ai-service/unit.md).
- Component: [`docs/tests/ai-service/component.md`](../../tests/ai-service/component.md).
