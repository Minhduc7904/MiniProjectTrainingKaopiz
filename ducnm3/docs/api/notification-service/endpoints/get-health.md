# `GET /health`

## Purpose

Checks that Notification Service is running and its owned Notification database accepts a lightweight query.

## Authentication and authorization

- Authentication: not required.
- Roles/scopes: none.
- Ownership rule: not applicable.

## Request

No path parameter, query parameter, or request body. Pagination does not apply.

## Success response

```http
200 OK
```

```json
{
  "data": {
    "service": "notification-service",
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

- `200`: Notification Service and `lms_notification_db` are available.
- `503 DATABASE_UNAVAILABLE`: Notification Service is running but cannot query its database.

## Business conditions and side effects

The endpoint executes `SELECT 1` against Notification Service's own database only. It does not write data or call another service.
