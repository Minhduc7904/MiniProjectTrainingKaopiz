# Get Failed Job Items

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/notification-jobs/{jobId}/failed-items?cursor=opaque-token&limit=100`

Success data payload `200 OK`:

```json
{"items":[{"studentId":"student-uuid","retryCount":1,"errorMessage":"Sender timeout"}],"nextCursor":"opaque-token"}
```

- Validate: job ID is UUID; limit 1–100; cursor opaque.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `404 NOTIFICATION_JOB_NOT_FOUND`.
