# `METHOD /api/resource`

## Purpose

State the user or system outcome and the owning service.

## Authentication and authorization

- Authentication: required or not required.
- Roles/scopes: allowed callers.
- Ownership rule: how the service confirms the caller may access the resource.

## Request

### Path and query parameters

Document each name, type, required flag, default, and validation rule.

### Body

```json
{
  "exampleField": "example value"
}
```

Document each body field, its type, required flag, validation, and business meaning.

## Success response

Use the envelope in [`../shared/response-format.md`](../shared/response-format.md).

```http
200 OK
```

```json
{
  "data": {
    "id": "example-id"
  },
  "meta": {
    "traceId": "01J..."
  }
}
```

For list endpoints, use the cursor or offset pagination envelope from the shared response format.

## Status codes

- `2xx`: success behavior.
- `400`: request validation failures.
- `401`: missing or invalid authentication.
- `403`: authorization or ownership failure.
- `404`: resource is not found.
- `409`: business or concurrency conflict.
- `5xx`: unexpected service or dependency failure.

List only codes that this endpoint can actually return and name the error code for each expected error.

## Business conditions and side effects

- Preconditions before execution.
- Database records created or changed.
- Events, background jobs, external HTTP calls, or MinIO operations.
- Idempotency, pagination, ordering, or retry behavior when applicable.
