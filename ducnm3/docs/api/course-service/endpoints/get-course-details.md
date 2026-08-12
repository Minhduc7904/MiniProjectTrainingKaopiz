# Get Course Details

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/courses/{courseId}/details`

Success data payload `200 OK`:

```json
{"id":"course-uuid","name":"Backend Fundamentals","lessons":[{"id":"lesson-uuid","title":"Introduction","displayOrder":1}]}
```

- Status: `401 UNAUTHENTICATED`, `403 COURSE_ACCESS_DENIED`, `404 COURSE_NOT_FOUND`.
- The implementation must avoid N+1 queries; document query count in benchmark documentation.
