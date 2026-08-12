# Delete Media

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`DELETE /api/media/{mediaId}`

Success data payload `204 No Content`.

- Validate: `mediaId` is UUID; only uploader/Admin may delete.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `404 MEDIA_NOT_FOUND`, `409 MEDIA_IN_USE`.
- Soft-deletes the metadata and schedules/executes object removal only after no active usage remains.
