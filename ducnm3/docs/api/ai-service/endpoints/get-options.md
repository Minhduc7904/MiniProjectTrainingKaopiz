# `GET /ai/api/ai/options` — Lấy lựa chọn cố định cho UI cấu hình

Business flow: [`get-options.md`](../../../business-flows/ai/get-options.md).

## Mục đích

Trả các loại AI kèm base URL mặc định và danh sách service được phép gán, để UI dựng form. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `GET /api/ai/options`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

Không có nội dung yêu cầu (body).

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
200 OK
```

```json
{
  "data": {
    "providerTypes": [
      {
        "providerType": "OPENAI",
        "defaultBaseUrl": "https://api.openai.com/v1"
      },
      {
        "providerType": "GEMINI",
        "defaultBaseUrl": "https://generativelanguage.googleapis.com/v1beta/openai/"
      },
      {
        "providerType": "ANTHROPIC",
        "defaultBaseUrl": "https://api.anthropic.com/v1/"
      },
      {
        "providerType": "MISTRAL",
        "defaultBaseUrl": "https://api.mistral.ai/v1"
      },
      {
        "providerType": "CUSTOM",
        "defaultBaseUrl": null
      }
    ],
    "bindableServices": [
      "course-service",
      "student-service",
      "media-service",
      "notification-service",
      "scheduler-service",
      "admin-service",
      "document-extraction",
      "lesson-synthesis",
      "quiz-authoring"
    ]
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```



## Mã trạng thái HTTP

- `200`: Thành công.
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "400",
    "message": "…",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Không truy cập database; dữ liệu cố định trong mã.

## Đồng bộ artifact

- Postman item: `AiService/GET Lấy lựa chọn cố định cho UI cấu hình` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
