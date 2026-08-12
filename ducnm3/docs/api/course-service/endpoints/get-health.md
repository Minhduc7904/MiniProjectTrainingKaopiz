# `GET /health`

## Purpose

Checks that Course Service is running and its owned Course database accepts a lightweight query.

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
    "service": "course-service",
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

- `200`: Course Service and `lms_course_db` are available.
- `503 DATABASE_UNAVAILABLE`: Course Service is running but cannot query its database.

## Business conditions and side effects

The endpoint executes `SELECT 1` against Course Service's own database only. It does not write data or call another service.
