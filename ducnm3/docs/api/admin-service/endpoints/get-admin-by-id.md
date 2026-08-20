# `GET /admin/api/admins/{adminId}`

Development-only Admin Service lookup used by Media Service to validate an
`ADMIN` actor. The service reads an allowlist from `Admin__Ids__0` configuration;
it does not provide login or password authentication in this slice.

## Response

```json
{
  "data": {
    "id": "00000000-0000-0000-0000-000000000001",
    "displayName": "Development Admin",
    "status": "ACTIVE"
  },
  "meta": { "traceId": "..." }
}
```

Unknown or inactive IDs return `404`. Invalid UUIDs return `400`.
