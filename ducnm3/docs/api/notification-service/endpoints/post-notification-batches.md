# `POST /api/notification-batches`

## Purpose

Creates a Notification Service-owned bulk delivery batch and snapshots its recipients. Execution is not implemented in the current foundation phase.

## Authentication and authorization

- Authentication: required.
- Roles/scopes: admin with notification broadcast permission.
- Ownership rule: Notification Service records the authenticated admin as `created_by`.

## Request

No path or query parameters.

```json
{
  "title": "Course update",
  "bodyMarkdown": "New material is available.",
  "targetScope": "COURSE_ENROLLED",
  "courseId": "2e71fdd3-a599-46d5-93e8-041e3b25b2b2",
  "batchSize": 500
}
```

- `title`: required string, maximum 200 characters.
- `bodyMarkdown`: required safe Markdown.
- `targetScope`: `COURSE_ENROLLED`, `STUDENT_IDS`, or `ALL_STUDENTS`.
- `courseId`: required UUID only for `COURSE_ENROLLED`.
- `batchSize`: integer from 100 through 1000.

## Success response

```http
202 Accepted
```

```json
{
  "data": {
    "id": "4c40bcf9-675e-435c-93bd-17cde82d1670",
    "status": "PENDING",
    "totalCount": 0
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

## Status codes

- `202`: batch and recipient snapshot were accepted.
- `400 VALIDATION_FAILED`: body or target scope is invalid.
- `401`: authentication is missing or invalid.
- `403`: caller cannot broadcast.
- `404 COURSE_NOT_FOUND`: requested Course does not exist.
- `409 BATCH_CONFLICT`: equivalent active batch conflicts with the request.
- `500 UNEXPECTED_ERROR`: safe unexpected failure response.

## Business conditions and side effects

Creates `notification_batches` and `notification_batch_items`. It does not create a Scheduler job yet; future `NOTIFICATION_BATCH_DISPATCH` integration will pass only the logical `batchId`. Retry and worker execution remain follow-up work.
