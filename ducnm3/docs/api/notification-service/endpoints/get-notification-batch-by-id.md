# `GET /api/notification-batches/{batchId}`

## Purpose

Returns Notification Service delivery counters and status for one bulk batch.

## Authentication and authorization

- Authentication: required.
- Roles/scopes: notification administrators.
- Ownership rule: access follows the owning organization's notification policy.

## Request

- `batchId`: required UUID path parameter.
- No query parameters or request body.

## Success response

```http
200 OK
```

```json
{
  "data": {
    "id": "4c40bcf9-675e-435c-93bd-17cde82d1670",
    "status": "PROCESSING",
    "totalCount": 3000,
    "processedCount": 1500,
    "successCount": 1490,
    "failedCount": 10
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Status codes

- `200`: batch found.
- `400 VALIDATION_FAILED`: `batchId` is not a UUID.
- `401`: authentication is missing or invalid.
- `403`: caller cannot inspect the batch.
- `404 NOTIFICATION_BATCH_NOT_FOUND`: batch does not exist.
- `500 UNEXPECTED_ERROR`: safe unexpected failure response.

## Business conditions and side effects

Reads `notification_batches` only and does not mutate state. Counts describe Notification business delivery, not generic Scheduler run history.
