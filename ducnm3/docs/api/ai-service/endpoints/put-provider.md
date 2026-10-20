# `PUT /ai/api/ai/providers/{providerId}` — Cập nhật AI provider

Business flow: [`put-provider.md`](../../../business-flows/ai/put-provider.md).

## Mục đích

Admin sửa tên, loại, endpoint, trạng thái và (tùy chọn) thay API key của provider. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `PUT /api/ai/providers/{providerId}`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Tham số đường dẫn

- `providerId` (UUID, bắt buộc, khác `00000000-0000-0000-0000-000000000000`)

### Nội dung yêu cầu

```json
{
  "name": "OpenAI production",
  "providerType": "OPENAI",
  "baseUrl": null,
  "apiKey": null,
  "status": "INACTIVE"
}
```

| Field | Bắt buộc | Quy tắc |
| --- | --- | --- |
| `name` | Có | Sau khi trim dài 2–100 ký tự; duy nhất không phân biệt hoa thường. |
| `providerType` | Có | Một trong `OPENAI`, `GEMINI`, `ANTHROPIC`, `MISTRAL`, `CUSTOM` (không phân biệt hoa thường, lưu chữ hoa). |
| `baseUrl` | Với `CUSTOM` | URL tuyệt đối `http(s)`, không chứa user/password, tối đa 500 ký tự. Bỏ trống thì dùng endpoint mặc định của loại AI (xem `GET /ai/api/ai/options`). |
| `apiKey` | Không | Không chứa khoảng trắng, tối đa 500 ký tự. Chỉ ghi, không bao giờ xuất hiện trong response, log hay Swagger example. |
| `status` | Có | `ACTIVE` hoặc `INACTIVE`. |

`apiKey` bỏ trống hoặc `null` nghĩa là **giữ nguyên key đang lưu**; chỉ khi có giá trị mới thì key được mã hóa lại và thay thế.

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
200 OK
```

```json
{
  "data": {
    "id": "3f2a8c1e-5b7d-4e2a-9c61-0a1b2c3d4e5f",
    "name": "OpenAI production",
    "providerType": "OPENAI",
    "baseUrl": "https://api.openai.com/v1",
    "apiKeyHint": "****1234",
    "status": "INACTIVE",
    "createdAtUtc": "2026-10-20T09:30:00Z",
    "updatedAtUtc": "2026-10-20T09:30:00Z"
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

Response của provider chỉ có `apiKeyHint` (`****` + 4 ký tự cuối); API key và ciphertext không bao giờ được trả về.

## Mã trạng thái HTTP

- `200`: Cập nhật thành công.
- `400`: `VALIDATION_FAILED`.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `404`: `AI_PROVIDER_NOT_FOUND`.
- `409`: `AI_PROVIDER_NAME_CONFLICT`: tên đã dùng bởi provider khác.

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "AI_PROVIDER_NAME_CONFLICT",
    "message": "…",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Cập nhật `ai_providers`; `updated_at` đổi.
- Binding hiện có không bị đổi; provider `INACTIVE` làm các service đã gán không dùng được AI ở Phase 2 (kiểm tra khi gọi).

## Đồng bộ artifact

- Postman item: `AiService/PUT Cập nhật AI provider` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
