# `DELETE /ai/api/ai/models/{modelId}` — Xóa AI model

API contract: [`delete-model.md`](../../api/ai-service/endpoints/delete-model.md)

## Mục tiêu

Admin xóa model chưa được gán cho service nào.

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

    Admin->>Gateway: DELETE /ai/api/ai/models/{modelId}
    Gateway->>API: Forward request
    API->>App: Kiểm tra actor Admin, validate, xử lý
    App->>DB: Đọc/ghi ai_models
    DB-->>App: Kết quả
    App-->>API: Response model
    API-->>Gateway: HTTP 204
    Gateway-->>Admin: HTTP response
```

## Luồng chính

1. Admin gửi `DELETE /ai/api/ai/models/{modelId}`.
2. Handler kiểm tra model tồn tại.
3. Repository xóa; FK vi phạm trả `409 AI_MODEL_IN_USE`.
4. API trả `204`.

## Luồng lỗi

- `400`: `VALIDATION_FAILED`.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_MODEL_NOT_FOUND`.
- `409`: `AI_MODEL_IN_USE`: model còn binding (FK `RESTRICT`).

## Dữ liệu và side effects

- Xóa 1 dòng `ai_models`. Phải gỡ binding trước.

## Test mapping

- Unit: [`docs/tests/ai-service/unit.md`](../../tests/ai-service/unit.md).
- Component: [`docs/tests/ai-service/component.md`](../../tests/ai-service/component.md).
