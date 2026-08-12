# Delete Media Usage

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`DELETE /api/media/usages/{usageId}`

Success data payload `204 No Content`.

- Validate: `usageId` is UUID; caller owns the usage owner.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 OWNER_ACCESS_DENIED`, `404 MEDIA_USAGE_NOT_FOUND`.
- Soft-deletes the usage; it does not delete the media object.
