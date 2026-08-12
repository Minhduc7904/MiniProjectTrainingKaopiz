# Get Media Content

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/media/{mediaId}/content`

Success data payload: `200 OK` binary stream with the stored safe MIME type, or `302 Found` to a short-lived presigned URL.

- Validate: `mediaId` is UUID and media is active.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 MEDIA_ACCESS_DENIED`, `404 MEDIA_NOT_FOUND`.
- Used by sanitized Markdown embeds; cache policy and authorization rules must be documented during implementation.
