# Get Media Metadata

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`GET /api/media/{mediaId}`

Success data payload `200 OK`:

```json
{"id":"media-uuid","mediaType":"IMAGE","contentType":"image/webp","sizeBytes":24576,"contentUrl":"/api/media/media-uuid/content"}
```

- Validate: `mediaId` is UUID.
- Status: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 MEDIA_ACCESS_DENIED`, `404 MEDIA_NOT_FOUND`.
- Never return `bucket` or `object_key`.
