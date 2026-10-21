# `POST /ai/api/ai/completions` — Chạy một lượt chat completion cho một tác vụ AI

Business flow: [`post-completions.md`](../../../business-flows/ai/post-completions.md).

## Mục đích

Service nội bộ (hiện là Course Service) nhờ AI Service gọi model đã gán cho một **khóa tác vụ** để sinh văn bản hoặc yêu cầu gọi tool (function calling). AI Service tra binding, giải mã API key, gọi `{baseUrl}/chat/completions` theo chuẩn OpenAI-compatible, ghi một dòng `ai_invocations` rồi trả kết quả. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `POST /api/ai/completions`.

AI Service **không** thực thi tool: model chỉ trả `toolCalls`, caller tự chạy tool rồi gửi kết quả ở lượt gọi kế tiếp (vòng lặp agent nằm ở caller).

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`. Course Service gọi bằng chính Admin đã yêu cầu tác vụ.
- Quy tắc sở hữu: cấu hình AI là dữ liệu dùng chung, không gắn với một actor.

## Yêu cầu

### Nội dung yêu cầu

```json
{
  "task": "lesson-synthesis",
  "messages": [
    { "role": "system", "text": "Bạn là người soạn bài giảng." },
    { "role": "user", "text": "<document name=\"bai-1.pdf\">…</document>" }
  ],
  "tools": [
    {
      "name": "save_lesson_content",
      "description": "Lưu bài học hoàn chỉnh.",
      "parametersSchema": {
        "type": "object",
        "properties": { "markdown": { "type": "string" } },
        "required": ["markdown"]
      }
    }
  ]
}
```

| Field | Bắt buộc | Quy tắc |
| --- | --- | --- |
| `task` | Có | Khóa binding: `document-extraction`, `lesson-synthesis`, `quiz-authoring` hoặc tên service được phép gán. |
| `messages` | Có | 1–200 message. `role` ∈ `system`, `user`, `assistant`, `tool`. Mỗi message có **hoặc** `text` **hoặc** `parts` (không cả hai), riêng `assistant` có thể chỉ có `toolCalls`. |
| `messages[].parts` | Không | Tối đa 64 phần. `type` ∈ `text`, `image`, `file`. `image`/`file` gửi `mediaType` và `dataBase64`; `file` (PDF) kèm `fileName`. |
| `messages[].toolCalls` | Không | Chỉ message `assistant`: `id`, `name`, `argumentsJson` (chuỗi JSON). |
| `messages[].toolCallId` | Có với `role=tool` | `id` của tool call mà message này trả lời. |
| `tools` | Không | Tối đa 16 tool, mỗi tool có `name` (chữ, số, `_`, `-`), `description`, `parametersSchema` là JSON Schema object. |

Tổng ký tự văn bản và base64 của một request tối đa 24 triệu ký tự; vượt thì `400`.

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
200 OK
```

```json
{
  "data": {
    "task": "lesson-synthesis",
    "providerType": "OPENAI",
    "modelName": "gpt-4o",
    "text": null,
    "toolCalls": [
      {
        "id": "call_abc123",
        "name": "save_lesson_content",
        "argumentsJson": "{\"markdown\":\"# Bài 1\\n…\"}"
      }
    ],
    "finishReason": "tool_calls",
    "promptTokens": 1840,
    "completionTokens": 912,
    "latencyMs": 6420
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

`finishReason = "length"` nghĩa là model bị cắt do hết token đầu ra; caller phải coi văn bản là chưa đầy đủ.

## Mã trạng thái HTTP

- `200`: Thành công (kể cả khi model chỉ trả `toolCalls`).
- `400`: `VALIDATION_FAILED`: `task` không hỗ trợ, `messages`/`tools` sai cấu trúc hoặc vượt giới hạn (`error.details` liệt kê từng field).
- `403`: Actor không phải Admin (header actor thiếu/sai định dạng trả `400`).
- `409`: `AI_TASK_NOT_CONFIGURED` (chưa gán model cho `task`), `AI_REFERENCE_INACTIVE` (model hoặc provider đang `INACTIVE`), `AI_PROVIDER_KEY_UNREADABLE` (không giải mã được API key đã lưu).
- `429`: `AI_PROVIDER_RATE_LIMITED`.
- `502`: `AI_PROVIDER_AUTH_FAILED` (key bị từ chối), `AI_PROVIDER_UNREACHABLE`, `AI_PROVIDER_REJECTED_REQUEST` (provider từ chối request, ví dụ model không hỗ trợ tool hoặc PDF; message chỉ chứa phần rút gọn, đã che API key), `AI_PROVIDER_INVALID_RESPONSE`.
- `504`: `AI_PROVIDER_TIMEOUT` (provider không trả lời trong 240 giây).

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "AI_TASK_NOT_CONFIGURED",
    "message": "No AI model is assigned to 'lesson-synthesis'. Assign one on the AI service bindings page.",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Binding của `task` phải tồn tại; model và provider phải `ACTIVE`.
- API key được giải mã chỉ trong bộ nhớ, gửi bằng `Authorization: Bearer` (provider `ANTHROPIC` gửi thêm `x-api-key` và `anthropic-version`). Key không bao giờ xuất hiện trong response, log hay `ai_invocations`.
- Không retry tự động: mỗi request gọi provider đúng một lần để tránh tính phí trùng; caller quyết định chạy lại.
- Ghi một dòng `ai_invocations` (task, model, provider, trạng thái, token, số tool call, độ trễ, mã lỗi) cho cả lần thành công và thất bại. Ghi log lỗi không làm mất kết quả đã có từ model.
- Nội dung `messages` (văn bản tài liệu, ảnh, PDF) chỉ đi qua tới provider, không được lưu trong AI Service.

## Đồng bộ artifact

- Postman item: `AiService/POST AI completion` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md) (`ai_invocations`).
- Runbook: [`docs/runbooks/ai-service.md`](../../../runbooks/ai-service.md).
