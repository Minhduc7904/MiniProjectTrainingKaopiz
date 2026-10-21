# `DELETE /ai/api/ai/service-bindings/{serviceName}` — Gỡ AI khỏi service

Business flow: [`delete-service-binding.md`](../../../business-flows/ai/delete-service-binding.md).

## Mục đích

Admin gỡ model AI đã gán cho service. Service sở hữu: AI Service. Đường dẫn trực tiếp của service là `DELETE /api/ai/service-bindings/{serviceName}`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` do Gateway/client gửi (cơ chế actor header của dự án).
- Vai trò: chỉ `ADMIN`. Actor không phải Admin trả `403`; thiếu hoặc sai định dạng header actor trả `400`.
- Quy tắc sở hữu: toàn bộ cấu hình AI là dữ liệu quản trị dùng chung, không gắn với một actor.

## Yêu cầu

### Tham số đường dẫn

- `serviceName` (string, bắt buộc): một trong `course-service`, `student-service`, `media-service`, `notification-service`, `scheduler-service`, `admin-service`, `document-extraction`, `lesson-synthesis`, `quiz-authoring`. Ba khóa cuối là khóa tác vụ AI của tính năng soạn nội dung Lesson (xem [`post-completions.md`](post-completions.md)). `ai-service` và gateway không được gán.

Không có nội dung yêu cầu (body).

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Mọi response có `Cache-Control: no-store`.

```http
204 No Content
```

Không có body.



## Mã trạng thái HTTP

- `204`: Gỡ thành công.
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

- Xóa 1 dòng `ai_service_bindings`; model và provider giữ nguyên.

## Đồng bộ artifact

- Postman item: `AiService/DELETE Gỡ AI khỏi service` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/ai-service/unit.md`](../../../tests/ai-service/unit.md), [`docs/tests/ai-service/component.md`](../../../tests/ai-service/component.md).
- Dữ liệu: [`docs/database/ai-service/data-model.md`](../../../database/ai-service/data-model.md).
