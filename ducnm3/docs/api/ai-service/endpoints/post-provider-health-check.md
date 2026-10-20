# `POST /ai/api/ai/providers/{providerId}/health-check` — Thử kết nối một AI provider

Business flow: [`post-provider-health-check.md`](../../../business-flows/ai/post-provider-health-check.md).

## Mục đích

Admin thử lại kết nối của một provider, ví dụ sau khi đổi API key hoặc base URL. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `POST /api/ai/providers/{providerId}/health-check`.

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
    "providerId": "3f2a8c1e-5b7d-4e2a-9c61-0a1b2c3d4e5f",
    "name": "OpenAI production",
    "providerType": "OPENAI",
    "baseUrl": "https://api.openai.com/v1",
    "providerStatus": "ACTIVE",
    "healthStatus": "ONLINE",
    "httpStatus": 200,
    "latencyMs": 412,
    "message": "Provider responded successfully.",
    "checkedAtUtc": "2026-10-20T09:30:00Z"
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

Trường kết quả:

- `healthStatus`: `ONLINE` (provider trả 2xx), `AUTH_FAILED` (HTTP 401/403, API key bị từ chối), `UNREACHABLE` (không kết nối được hoặc quá thời gian chờ), `ERROR` (HTTP lỗi khác, base URL không hợp lệ, hoặc API key đã lưu không giải mã được), `DISABLED` (provider `INACTIVE`, không gọi ra ngoài).
- `httpStatus`/`latencyMs`: mã HTTP và thời gian phản hồi của lần gọi thử; `null` khi không có (`DISABLED`, mất kết nối).
- `message`: mô tả ngắn, không chứa API key, host nội bộ hay nội dung response của provider.
- Không có API key, hint hay ciphertext nào trong response ngoài thông tin provider; kết quả **không được lưu** vào database.

## Mã trạng thái HTTP

- `200`: Thành công (kể cả khi provider không kết nối được; trạng thái nằm trong `healthStatus`).
- `400`: `VALIDATION_FAILED` khi `providerId` không phải UUID hợp lệ.
- `404`: `AI_PROVIDER_NOT_FOUND`.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).

## Điều kiện nghiệp vụ và tác động phụ

- Giải mã API key đã lưu rồi gọi `GET {baseUrl}/models` với `Authorization: Bearer <key>` (provider `ANTHROPIC` gửi thêm `x-api-key` và `anthropic-version`). Timeout 6 giây, không theo redirect.
- Provider `INACTIVE` không bị gọi ra ngoài.
- Không ghi database.

## Đồng bộ artifact

- Postman item: `AiService/POST Kiểm tra kết nối AI provider` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
