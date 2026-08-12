# Create Course

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`POST /api/courses`

Request:

```json
{"name":"Backend Fundamentals","descriptionMarkdown":"# Overview","status":"DRAFT"}
```

Success data payload `201 Created`:

```json
{"id":"course-uuid","name":"Backend Fundamentals","status":"DRAFT","createdAt":"2026-08-12T06:00:00Z"}
```

- Validate: `name` required, 3–200 characters; `status` is `DRAFT`, `PUBLISHED`, or `ARCHIVED`; Markdown has no unsafe HTML.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `409 COURSE_NAME_CONFLICT`.
- Side effect: creates `courses`. Media is registered through Media Service separately.
