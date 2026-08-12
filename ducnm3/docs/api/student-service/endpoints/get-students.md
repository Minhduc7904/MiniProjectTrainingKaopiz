# List Students

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/students?status=ACTIVE&page=1&pageSize=20`

Success data payload `200 OK`:

```json
{"items":[{"id":"student-uuid","email":"student@example.com","displayName":"Student One","status":"ACTIVE"}],"page":1,"pageSize":20,"total":1}
```

- Validate: `status` optional valid enum; `page >= 1`; `pageSize` is 1–100.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`.
