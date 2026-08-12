# Send Single Notification

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`POST /api/notifications`

Request:

```json
{"studentId":"student-uuid","title":"Course update","bodyMarkdown":"New material is available."}
```

Success data payload `201 Created`:

```json
{"id":"notification-uuid","recipientStudentId":"student-uuid","status":"UNREAD","sourceType":"SINGLE"}
```

- Validate: IDs are UUID; title required, 1–200 characters; Markdown is sanitized and media URLs are owned by Media Service.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `404 STUDENT_NOT_FOUND`.
- Side effect: creates one `notifications` record with `source_type = SINGLE`.
