# Mark All Notifications Read

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`POST /api/notifications/read-all`

Success data payload `200 OK`:

```json
{"updatedCount":12,"readAt":"2026-08-12T06:05:00Z"}
```

- Validate: no recipient parameter is accepted.
- Status: `401 UNAUTHENTICATED`.
- Side effect: updates only the caller's `UNREAD` inbox rows; operation is idempotent.
