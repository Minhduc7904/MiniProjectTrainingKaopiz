# `GET /course/api/courses/{courseId}/lessons/{lessonId}/ai-documents` — Trạng thái trích xuất tài liệu của Lesson

API contract: [`GET Trạng thái trích xuất tài liệu`](../../api/course-service/endpoints/get-course-lesson-ai-documents.md)

## Mục tiêu

Cho UI hiển thị (và poll) trạng thái trích xuất của từng tài liệu đính kèm mà không ghi dữ liệu.

## Actor và thành phần

- Admin; API Gateway; Course Service; Media Service; MySQL `lms_course_db`.

## Điều kiện trước

- Header `X-Actor-Type: ADMIN`, `X-Actor-Id` hợp lệ; Lesson thuộc Course.

## UML luồng chạy

```mermaid
sequenceDiagram
    participant Admin
    participant Gateway
    participant API as Course Service API
    participant Media as Media Service
    participant DB as MySQL lms_course_db

    Admin->>Gateway: GET /course/api/courses/{courseId}/lessons/{lessonId}/ai-documents
    Gateway->>API: Forward request
    API->>DB: Lesson thuộc Course?
    alt không có Lesson
        API-->>Admin: 404 LESSON_NOT_FOUND
    else có Lesson
        API->>Media: GET /api/media/usages/urls
        Media-->>API: Tệp đính kèm
        API->>DB: Đọc lesson_attachment_texts (không tải extracted_text)
        API-->>Admin: 200 items (NOT_QUEUED nếu chưa có bản ghi)
    end
```

## Luồng chính

1. Kiểm tra Lesson, đọc tệp đính kèm và bản ghi trích xuất.
2. Hợp nhất: tệp chưa có bản ghi hiện `NOT_QUEUED` (hoặc `UNSUPPORTED` nếu định dạng không hỗ trợ).
3. Trả `200` với `no-store`.

## Luồng lỗi

- `400`, `403`, `404 LESSON_NOT_FOUND`, `500 UNEXPECTED_ERROR` khi Media Service lỗi.

## Dữ liệu và side effects

- Không ghi dữ liệu. Không trả văn bản đã trích xuất.

## Test mapping

- Unit: [`docs/tests/course-service/unit.md`](../../tests/course-service/unit.md) (`LessonAiHandlersTests`, `DocumentExtractionTests`, `LessonAiJobProcessingTests`).
- Component: [`docs/tests/course-service/component.md`](../../tests/course-service/component.md) (`LessonAiEndpointsComponentTests`).
