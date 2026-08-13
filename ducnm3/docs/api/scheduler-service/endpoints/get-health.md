# `GET /health`

## Purpose

Checks that Scheduler Service is running and its owned Scheduler database accepts a lightweight query.

## Authentication and authorization

- Authentication: not required.
- Roles/scopes: none.
- Ownership rule: not applicable.

## Request

No path parameter, query parameter, request body, or pagination.

## Success response

```http
200 OK
```

```json
{
  "data": {
    "service": "scheduler-service",
    "status": "healthy",
    "database": {
      "status": "healthy"
    }
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Status codes

- `200`: Scheduler Service and `lms_scheduler_db` are available.
- `503 DATABASE_UNAVAILABLE`: API is running but cannot query its database.

## Business conditions and side effects

Executes `SELECT 1` against `lms_scheduler_db` only. It does not create, claim, parse, or execute jobs and does not call another service.
