# List My Inbox

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/notifications/me?status=UNREAD&cursor=opaque-token&limit=20`

Success data payload `200 OK`:

```json
{"items":[{"id":"notification-uuid","title":"Course update","bodyMarkdown":"New material is available.","status":"UNREAD","createdAt":"2026-08-12T06:00:00Z"}],"nextCursor":"opaque-token"}
```

- Validate: `status` optional `UNREAD|READ`; limit 1–100; cursor opaque.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`.
- Side effects: none; recipient is always derived from identity.
