# Create Lesson

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`POST /api/courses/{courseId}/lessons`

Request:

```json
{"title":"Introduction","displayOrder":1,"contentMarkdown":"## Start here"}
```

Success data payload `201 Created`:

```json
{"id":"lesson-uuid","courseId":"course-uuid","displayOrder":1}
```

- Validate: Course exists; title required; `displayOrder >= 1`; Markdown is safe.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `404 COURSE_NOT_FOUND`, `409 LESSON_ORDER_CONFLICT`.
- Side effect: creates `lessons`.
