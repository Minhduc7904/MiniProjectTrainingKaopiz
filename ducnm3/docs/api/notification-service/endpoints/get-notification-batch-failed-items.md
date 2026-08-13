# `GET /api/notification-batches/{batchId}/failed-items`

## Purpose

Lists failed recipient items for a Notification Service bulk batch.

## Authentication and authorization

- Authentication: required.
- Roles/scopes: notification administrators.
- Ownership rule: access follows the owning organization's notification policy.

## Request

- `batchId`: required UUID path parameter.
- `cursor`: optional opaque continuation token.
- `limit`: optional integer from 1 through 100; default 100.
- No request body.

## Success response

Ordering is stable by item ID. Clients must treat `nextCursor` as opaque.

```http
200 OK
```

```json
{
  "data": {
    "items": [
      {
        "studentId": "4691356d-12d2-44e1-a7d1-cad9959c4bf3",
        "retryCount": 1,
        "errorMessage": "Sender timeout"
      }
    ]
  },
  "meta": {
    "traceId": "01J...",
    "pagination": {
      "type": "cursor",
      "limit": 100,
      "nextCursor": "opaque-token",
      "hasNextPage": true
    }
  }
}
```

## Status codes

- `200`: failed items returned.
- `400 VALIDATION_FAILED`: ID, limit, or cursor is invalid.
- `401`: authentication is missing or invalid.
- `403`: caller cannot inspect the batch.
- `404 NOTIFICATION_BATCH_NOT_FOUND`: batch does not exist.
- `500 UNEXPECTED_ERROR`: safe unexpected failure response.

## Business conditions and side effects

Reads `notification_batch_items` with `status = FAILED`. The endpoint does not trigger retry or create Scheduler runs.
