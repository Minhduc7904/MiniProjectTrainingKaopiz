# Get Course

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/courses/{courseId}`

Success data payload `200 OK`:

```json
{"id":"course-uuid","name":"Backend Fundamentals","descriptionMarkdown":"# Overview","status":"PUBLISHED"}
```

- Validate: `courseId` is UUID.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 COURSE_ACCESS_DENIED`, `404 COURSE_NOT_FOUND`.
- Student may only read an accessible published Course.
