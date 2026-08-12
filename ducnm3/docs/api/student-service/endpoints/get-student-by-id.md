# Get Student

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/students/{studentId}`

Success data payload `200 OK`:

```json
{"id":"student-uuid","email":"student@example.com","displayName":"Student One","status":"ACTIVE"}
```

- Validate: `studentId` is UUID.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 STUDENT_ACCESS_DENIED`, `404 STUDENT_NOT_FOUND`.
