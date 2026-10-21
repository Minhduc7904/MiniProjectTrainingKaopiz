# `POST /course/api/courses/{courseId}/lessons/{lessonId}/ai-jobs` — Tạo job AI tổng hợp bài học hoặc soạn quiz

Business flow: [`post-course-lesson-ai-jobs.md`](../../../business-flows/course-learning/post-course-lesson-ai-jobs.md).

## Mục đích

Admin chọn một số tài liệu đã trích xuất và yêu cầu AI **tổng hợp thành nội dung Lesson** (`SYNTHESIZE_LESSON`) hoặc **soạn quiz trắc nghiệm** (`GENERATE_QUIZ`). Endpoint chỉ tạo job rồi trả ngay `202 Accepted`; worker nền chạy job vì gọi LLM kéo dài hàng chục giây. Service sở hữu: Course Service. Đường dẫn trực tiếp của service là `POST /api/courses/{courseId}/lessons/{lessonId}/ai-jobs`.

Model tự ghi kết quả bằng cách gọi tool (`save_lesson_content`, `create_quiz`); các tool chỉ ghi vào đúng Lesson của job và Quiz luôn là `DRAFT`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` (UUID hợp lệ).
- Vai trò: chỉ `ADMIN`. Actor khác trả `403 FORBIDDEN`; thiếu hoặc sai định dạng header trả `400`.
- `courseId`, `lessonId` phải là UUID khác rỗng, sai trả `400 VALIDATION_FAILED`.

## Yêu cầu

### Tham số đường dẫn

`courseId`, `lessonId` (UUID, bắt buộc).

### Nội dung yêu cầu

```json
{
  "jobType": "GENERATE_QUIZ",
  "mediaIds": ["6f1c2b3a-1d4e-4a5b-8c9d-0e1f2a3b4c5d"],
  "questionCount": 10,
  "overwriteContent": false
}
```

| Field | Bắt buộc | Quy tắc |
| --- | --- | --- |
| `jobType` | Có | `SYNTHESIZE_LESSON` hoặc `GENERATE_QUIZ`. |
| `mediaIds` | Có | 1–20 UUID tài liệu của Lesson, mỗi tài liệu phải `EXTRACTED`. ID trùng được gộp. |
| `questionCount` | Không | Chỉ dùng cho `GENERATE_QUIZ`: 1–30, mặc định 10. Bị bỏ qua với `SYNTHESIZE_LESSON`. |
| `overwriteContent` | Không | Mặc định `false`. Với `SYNTHESIZE_LESSON` khi Lesson đã có nội dung, phải là `true` mới được tạo job. |

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Response có `Cache-Control: no-store` và header `Location` trỏ tới chi tiết job.

```http
202 Accepted
```

```json
{
  "data": {
    "id": "9a8b7c6d-5e4f-4a3b-9c2d-1e0f9a8b7c6d",
    "lessonId": "22222222-2222-2222-2222-222222222222",
    "courseId": "11111111-1111-1111-1111-111111111111",
    "jobType": "GENERATE_QUIZ",
    "status": "PENDING",
    "mediaIds": ["6f1c2b3a-1d4e-4a5b-8c9d-0e1f2a3b4c5d"],
    "questionCount": 10,
    "overwriteContent": false,
    "hasPreviousContent": false,
    "resultQuizId": null,
    "stepCount": 0,
    "errorCode": null,
    "errorMessage": null,
    "createdAtUtc": "2026-10-21T08:20:00Z",
    "updatedAtUtc": "2026-10-21T08:20:00Z",
    "startedAtUtc": null,
    "finishedAtUtc": null
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

| Field | Ý nghĩa |
| --- | --- |
| `jobType` | `SYNTHESIZE_LESSON` (ghi Markdown vào nội dung Lesson) hoặc `GENERATE_QUIZ` (tạo Quiz `DRAFT`). |
| `status` | `PENDING` → `RUNNING` → `COMPLETED` / `FAILED`. |
| `mediaIds` | Các tài liệu admin đã chọn làm nguồn. |
| `questionCount` | Số câu yêu cầu khi `GENERATE_QUIZ`; `null` với `SYNTHESIZE_LESSON`. |
| `overwriteContent` | Admin đã xác nhận cho ghi đè nội dung Lesson đang có. |
| `hasPreviousContent` | `true` khi job đã ghi đè và lưu bản nội dung cũ (lấy qua endpoint chi tiết job). |
| `resultQuizId` | UUID Quiz `DRAFT` do job tạo khi `GENERATE_QUIZ` hoàn tất. |
| `stepCount` | Số lượt gọi model của job. |
| `errorCode`, `errorMessage` | Chỉ có khi `FAILED`; thông điệp an toàn để hiển thị. |
| `startedAtUtc`, `finishedAtUtc` | Mốc worker bắt đầu / kết thúc; `null` khi chưa tới. |

## Mã trạng thái HTTP

- `202`: Đã tạo job `PENDING`.
- `400`: `VALIDATION_FAILED`: `jobType`, `mediaIds` hoặc `questionCount` sai (`error.details` liệt kê field), UUID sai hoặc thiếu header actor.
- `403`: Actor không phải Admin.
- `404`: `LESSON_NOT_FOUND`.
- `409`: `LESSON_CONTENT_EXISTS` (Lesson đã có nội dung mà `overwriteContent` không phải `true`), `LESSON_AI_DOCUMENT_NOT_READY` (có tài liệu chưa `EXTRACTED` hoặc không thuộc Lesson), `LESSON_AI_JOB_ACTIVE` (đã có job cùng loại đang `PENDING`/`RUNNING` trong Lesson).

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md), ví dụ:

```json
{
  "error": {
    "code": "LESSON_CONTENT_EXISTS",
    "message": "Lesson already has content. Set overwriteContent to true to replace it.",
    "details": []
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

## Điều kiện nghiệp vụ và tác động phụ

- Mỗi Lesson tối đa một job đang chờ/chạy cho mỗi loại; hai loại có thể chạy song song.
- Chèn `lesson_ai_jobs` (`PENDING`). Không gọi AI Service trong request này.
- Worker nền nhận job, gom văn bản các tài liệu đã chọn (tối đa 150.000 ký tự, vượt thì job `FAILED` `DOCUMENTS_TOO_LARGE`), gọi AI Service task `lesson-synthesis` hoặc `quiz-authoring` theo vòng lặp tối đa 6 lượt.
- `SYNTHESIZE_LESSON`: tool `save_lesson_content` ghi qua `UpdateLessonHandler` (đồng bộ media usage như khi sửa Lesson). Nếu Lesson đã có nội dung, bản cũ được lưu vào `lesson_ai_jobs.previous_content_markdown` trước khi ghi đè.
- `GENERATE_QUIZ`: tool `create_quiz` tạo Quiz qua `CreateQuizHandler` với `status = DRAFT` (model không thể xuất bản), đúng `questionCount` câu; lỗi validation của quiz được trả lại cho model để tự sửa.
- Nội dung tài liệu được đặt trong khối `<document>` và system prompt chỉ rõ đó là dữ liệu, không phải chỉ dẫn (giảm prompt injection). Mọi ID (`lessonId`, `courseId`) do server gắn, không lấy từ model.
- Job bị bỏ dở (worker chết khi `RUNNING`) được đánh dấu `FAILED` (`WORKER_INTERRUPTED`) khi hết lease, **không** chạy lại để tránh tạo trùng Quiz; admin tạo job mới.

## Đồng bộ artifact

- Postman item: `CourseService/POST Create lesson AI job` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/course-service/unit.md`](../../../tests/course-service/unit.md), [`docs/tests/course-service/component.md`](../../../tests/course-service/component.md).
- Dữ liệu: [`docs/database/course-service/data-model.md`](../../../database/course-service/data-model.md) (`lesson_ai_jobs`).
- AI Service: [`post-completions.md`](../../ai-service/endpoints/post-completions.md).
- Runbook: [`docs/runbooks/lesson-ai-authoring.md`](../../../runbooks/lesson-ai-authoring.md).
