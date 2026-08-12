# Export Courses

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/courses/export?status=PUBLISHED`

Success data payload `200 OK` with `Content-Type: text/csv`:

```csv
id,name,status
course-uuid,Backend Fundamentals,PUBLISHED
```

- Validate: optional `status` is valid.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`.
- Stream output; do not load the full export into memory.
