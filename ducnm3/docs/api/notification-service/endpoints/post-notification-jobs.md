# Create Bulk Notification Job

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`POST /api/notification-jobs`

Request:

```json
{"title":"Course update","bodyMarkdown":"New material is available.","targetScope":"COURSE_ENROLLED","courseId":"course-uuid","batchSize":500}
```

Success data payload `202 Accepted`:

```json
{"id":"job-uuid","status":"PENDING","totalCount":0}
```

- Validate: title required; Markdown safe; target scope valid; `courseId` required for `COURSE_ENROLLED`; batch size 100–1000.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `404 COURSE_NOT_FOUND`, `409 JOB_CONFLICT`.
- Side effects: creates `notification_jobs`, snapshots recipients into `notification_job_items`, and schedules background processing.
