# `POST /ai/api/ai/providers/health-checks` — Thử kết nối tất cả AI provider

Business flow: [`post-provider-health-checks.md`](../../../business-flows/ai/post-provider-health-checks.md).

## Mục đích

Admin thử gọi tất cả AI provider một lần để xem provider nào đang dùng được; trang AI provider và Dashboard gọi endpoint này mỗi lần mở trang. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `POST /api/ai/providers/health-checks`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

Không có tham số đường dẫn hoặc query.

Không có nội dung yêu cầu (body).

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
200 OK
```

```json
{
  "data": [
    {
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
    {
      "providerId": "7c9d1a42-0e55-4b3a-8d27-5f6a7b8c9d0e",
      "name": "Mistral dự phòng",
      "providerType": "MISTRAL",
      "baseUrl": "https://api.mistral.ai/v1",
      "providerStatus": "INACTIVE",
      "healthStatus": "DISABLED",
      "httpStatus": null,
      "latencyMs": null,
      "message": "Provider is inactive, connection was not tested.",
      "checkedAtUtc": "2026-10-20T09:30:00Z"
    }
  ],
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
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).

## Điều kiện nghiệp vụ và tác động phụ

- Giải mã API key đã lưu rồi gọi `GET {baseUrl}/models` với `Authorization: Bearer <key>` (provider `ANTHROPIC` gửi thêm `x-api-key` và `anthropic-version`). Timeout 6 giây, không theo redirect.
- Provider `INACTIVE` không bị gọi ra ngoài.
- Các provider được gọi song song (tối đa 8 cuộc gọi cùng lúc), danh sách trả theo thứ tự mới tạo trước; không phân trang vì chỉ có vài provider.
- Không ghi database.

## Đồng bộ artifact

- Postman item: `AiService/POST Kiểm tra kết nối tất cả AI provider` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
