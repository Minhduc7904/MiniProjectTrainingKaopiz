# `GET /course/api/courses/{courseId}/lessons/{lessonId}/ai-jobs/{jobId}` — Chi tiết một job AI của Lesson

Business flow: [`get-course-lesson-ai-job-by-id.md`](../../../business-flows/course-learning/get-course-lesson-ai-job-by-id.md).

## Mục đích

Trả một job AI kèm nội dung Lesson cũ nếu job đã ghi đè, để admin khôi phục khi kết quả AI không đạt. Service sở hữu: Course Service. Đường dẫn trực tiếp của service là `GET /api/courses/{courseId}/lessons/{lessonId}/ai-jobs/{jobId}`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` (UUID hợp lệ).
- Vai trò: chỉ `ADMIN`. Actor khác trả `403 FORBIDDEN`; thiếu hoặc sai định dạng header trả `400`.
- `courseId`, `lessonId`, `jobId` phải là UUID khác rỗng, sai trả `400 VALIDATION_FAILED`.

## Yêu cầu

`courseId`, `lessonId`, `jobId` (UUID, bắt buộc). `jobId` phải thuộc Lesson trên đường dẫn.

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Response có `Cache-Control: no-store`.

```http
200 OK
```

```json
{
  "data": {
    "job": {
        "id": "9a8b7c6d-5e4f-4a3b-9c2d-1e0f9a8b7c6d",
        "lessonId": "22222222-2222-2222-2222-222222222222",
        "courseId": "11111111-1111-1111-1111-111111111111",
        "jobType": "GENERATE_QUIZ",
        "status": "COMPLETED",
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
    "previousContentMarkdown": null
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

`job` có cùng field với [`POST …/ai-jobs`](post-course-lesson-ai-jobs.md). `previousContentMarkdown` chỉ khác `null` khi `job.hasPreviousContent = true`.

## Mã trạng thái HTTP

- `200`: Thành công.
- `400`: `VALIDATION_FAILED` (UUID sai) hoặc thiếu header actor.
- `403`: Actor không phải Admin.
- `404`: `LESSON_NOT_FOUND` hoặc `LESSON_AI_JOB_NOT_FOUND` (không tồn tại hoặc thuộc Lesson khác).

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

- Không ghi dữ liệu. Đọc `lesson_ai_jobs`; chỉ đọc `previous_content_markdown` khi cần.

## Đồng bộ artifact

- Postman item: `CourseService/GET Lesson AI job detail` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/course-service/unit.md`](../../../tests/course-service/unit.md), [`docs/tests/course-service/component.md`](../../../tests/course-service/component.md).
- Dữ liệu: [`docs/database/course-service/data-model.md`](../../../database/course-service/data-model.md).
