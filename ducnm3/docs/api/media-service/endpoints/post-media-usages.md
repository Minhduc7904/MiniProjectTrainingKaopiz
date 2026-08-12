# Create Media Usage

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`POST /api/media/usages`

Request:

```json
{"mediaId":"media-uuid","ownerService":"COURSE","ownerType":"COURSE_THUMBNAIL","ownerId":"course-uuid","usageType":"THUMBNAIL"}
```

Success data payload `201 Created`:

```json
{"id":"usage-uuid","mediaId":"media-uuid","ownerService":"COURSE","ownerType":"COURSE_THUMBNAIL","ownerId":"course-uuid","usageType":"THUMBNAIL"}
```

- Validate: all IDs are UUID; owner enum combination is allowed; media is active.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 OWNER_ACCESS_DENIED`, `404 MEDIA_NOT_FOUND`, `409 MEDIA_USAGE_CONFLICT`.
- For `COURSE_THUMBNAIL`, replaces the previous active thumbnail atomically.
