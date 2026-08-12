# Seed Students

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`POST /api/students/seed`

Request:

```json
{"count":3000}
```

Success data payload `202 Accepted`:

```json
{"requestedCount":3000,"status":"ACCEPTED"}
```

- Validate: `count` is 1–100000.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `409 SEED_ALREADY_RUNNING`.
- Side effect: creates Student records; implementation must state whether execution is synchronous or background.
