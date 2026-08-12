# Upload Media

## Response standard

JSON success responses use the shared envelope in [`../shared/response-format.md`](../../shared/response-format.md). The concrete JSON below is the value of `data`; add `meta` for `traceId` and pagination. CSV and binary streaming endpoints are exceptions.

`POST /api/media` (`multipart/form-data`)

Request fields: `file`, `mediaType`.

Success data payload `201 Created`:

```json
{"id":"media-uuid","mediaType":"IMAGE","contentType":"image/webp","sizeBytes":24576,"contentUrl":"/api/media/media-uuid/content"}
```

- Validate: file required; allowed MIME matches `mediaType`; size is within configured limit; checksum is calculated server-side.
- Status: `400 INVALID_MEDIA`, `401 UNAUTHENTICATED`, `413 MEDIA_TOO_LARGE`, `415 UNSUPPORTED_MEDIA_TYPE`.
- Side effects: writes binary to MinIO and metadata to `media_objects`.
