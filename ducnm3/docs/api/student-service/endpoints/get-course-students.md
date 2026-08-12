# Get Students Enrolled in Course

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/students/course/{courseId}?cursor=opaque-token&limit=500`

Success data payload `200 OK`:

```json
{"items":[{"id":"student-uuid","status":"ACTIVE"}],"nextCursor":"opaque-token"}
```

- Internal endpoint used by Notification Service for bulk recipients.
- Validate: `courseId` is UUID; `limit` is 1–1000; cursor is opaque.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 INTERNAL_SERVICE_ONLY`, `404 COURSE_NOT_FOUND`.
- Must return a stable, deterministic ordering to support idempotent job snapshots.
