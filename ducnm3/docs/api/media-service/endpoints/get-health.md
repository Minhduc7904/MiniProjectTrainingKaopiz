# `GET /health`

## Purpose

Checks that Media Service can reach its owned database and that MinIO contains
all five configured media buckets.

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
    "service": "media-service",
    "status": "healthy",
    "database": {
      "status": "healthy"
    },
    "storage": {
      "status": "healthy"
    }
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Status codes

- `200`: the database and MinIO are available.
- `503 DATABASE_UNAVAILABLE`: only the Media database probe failed.
- `503 STORAGE_UNAVAILABLE`: only the MinIO probe failed or a required bucket is missing.
- `503 DEPENDENCY_UNAVAILABLE`: both database and MinIO probes failed.

All `503` responses use the shared error envelope:

```json
{
  "error": {
    "code": "STORAGE_UNAVAILABLE",
    "message": "Storage is temporarily unavailable.",
    "details": []
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Business conditions and side effects

The endpoint runs the database and storage probes concurrently. The database
probe executes `SELECT 1`. The storage probe checks MinIO connectivity and the
existence of `images`, `videos`, `documents`, `audios`, and `other` with a short
timeout. Neither probe writes data or uploads a test object.
