# List Courses

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/courses?status=PUBLISHED&page=1&pageSize=20`

Success data payload `200 OK`:

```json
{"items":[{"id":"course-uuid","name":"Backend Fundamentals","status":"PUBLISHED"}],"page":1,"pageSize":20,"total":1}
```

- Validate: `page >= 1`; `pageSize` is 1–100; `status` is optional valid enum.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`.
- Ordering must be documented when implementation is added.
