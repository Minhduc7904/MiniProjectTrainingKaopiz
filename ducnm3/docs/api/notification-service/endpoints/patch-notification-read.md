# Mark Notification Read

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`PATCH /api/notifications/{notificationId}/read`

Success data payload `200 OK`:

```json
{"id":"notification-uuid","status":"READ","readAt":"2026-08-12T06:05:00Z"}
```

- Validate: notification ID is UUID.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 NOTIFICATION_ACCESS_DENIED`, `404 NOTIFICATION_NOT_FOUND`.
- Idempotent: repeated calls return the same read state without changing the original `read_at`.
