# Get Notification Job

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/notification-jobs/{jobId}`

Success data payload `200 OK`:

```json
{"id":"job-uuid","status":"PROCESSING","totalCount":3000,"processedCount":1500,"successCount":1490,"failedCount":10}
```

- Validate: `jobId` is UUID.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `404 NOTIFICATION_JOB_NOT_FOUND`.
