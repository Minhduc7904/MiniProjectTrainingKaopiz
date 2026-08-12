# List Media Usages

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/media/usages?ownerService=COURSE&ownerType=COURSE_THUMBNAIL&ownerId=course-uuid`

Success data payload `200 OK`:

```json
{"items":[{"id":"usage-uuid","mediaId":"media-uuid","usageType":"THUMBNAIL","contentUrl":"/api/media/media-uuid/content"}]}
```

- Validate: owner service/type combination is allowed; `ownerId` is UUID.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 OWNER_ACCESS_DENIED`.
- Uses `display_order` for deterministic ordering.
