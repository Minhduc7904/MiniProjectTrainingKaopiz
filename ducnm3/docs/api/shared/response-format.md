# Shared Response Format

All JSON endpoints use this envelope. CSV export, file streams, and redirects to presigned media URLs are explicit non-JSON exceptions.

## Successful single resource or command

```json
{
  "data": {
    "id": "course-uuid",
    "name": "Backend Fundamentals"
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

- `data`: requested resource, command result, or `null` for an intentional empty success.
- `meta.traceId`: correlation ID for logs and support; always included in production responses.

## Cursor pagination

```json
{
  "data": [
    {
      "id": "notification-uuid",
      "title": "Course update"
    }
  ],
  "meta": {
    "traceId": "01J...",
    "pagination": {
      "type": "cursor",
      "limit": 20,
      "nextCursor": "opaque-token",
      "hasNextPage": true
    }
  }
}
```

- `nextCursor` is `null` when no next page exists.
- Cursor is opaque, must not expose database IDs or internal ordering fields.
- The endpoint defines and documents a stable ordering.

## Offset pagination

```json
{
  "data": [
    {
      "id": "course-uuid",
      "name": "Backend Fundamentals"
    }
  ],
  "meta": {
    "traceId": "01J...",
    "pagination": {
      "type": "offset",
      "page": 1,
      "pageSize": 20,
      "totalItems": 101,
      "totalPages": 6
    }
  }
}
```

- `page` starts at 1.
- `pageSize` is the requested, validated size.
- `totalItems` and `totalPages` are required only for offset pagination.

## Errors

```json
{
  "error": {
    "code": "COURSE_NOT_FOUND",
    "message": "Course not found",
    "details": []
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

- `error.code`: stable application error code.
- `error.message`: safe message for API consumers.
- `error.details`: optional validation detail array; omit when no details apply.
- Never expose stack traces, SQL, credentials, MinIO bucket names, or object keys.

## Service health

Each service exposes `GET /health`. It has no request body, query parameters, or pagination.

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

- `200`: the HTTP service is running and its owned database accepts `SELECT 1`.
- `503 DATABASE_UNAVAILABLE`: the HTTP service is running but its owned database cannot be reached.
- `503 SERVICE_UNAVAILABLE`: API Gateway cannot connect to a downstream service.
