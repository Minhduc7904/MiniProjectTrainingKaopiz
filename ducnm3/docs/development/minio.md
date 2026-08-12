# MinIO development guide

## Ownership and current scope

Only Media Service may access MinIO. `MediaService.Application` owns the
`IStorage` and `IStorageHealthProbe` ports; `MediaService.Infrastructure`
implements both ports with the MinIO SDK.

This setup includes storage operations, validation, dependency injection,
provisioning, health checks, and tests. It intentionally does not expose an
upload/download HTTP endpoint or write `media_objects` rows yet.

## Buckets and object keys

Callers pass a media category, never an arbitrary upload bucket.

| Category | Bucket |
| --- | --- |
| `IMAGE` | `images` |
| `VIDEO` | `videos` |
| `DOCUMENT` | `documents` |
| `AUDIO` | `audios` |
| `OTHER` | `other` |

Object keys use the UTC upload date and a generated UUID:

```text
yyyy/MM/dd/{uuid}.{extension}
```

For example, an image can be stored as:

```text
bucket: images
object_key: 2026/08/12/619319269e3946dab81657242c11bc86.png
storage_address: images/2026/08/12/619319269e3946dab81657242c11bc86.png
```

The database stores the bucket and object key separately. Bucket names and
object keys are internal references and must not become public URLs.

## Configuration

Copy `.env.example` to `.env` and replace the local placeholder credentials.
The relevant variables are:

```dotenv
MINIO_ROOT_USER=minio-root-user
MINIO_ROOT_PASSWORD=replace-with-a-long-root-secret
MINIO_APP_ACCESS_KEY=media-storage-app
MINIO_APP_SECRET_KEY=replace-with-a-long-app-secret
MINIO_USE_SSL=false
MINIO_HEALTH_TIMEOUT_SECONDS=3
MINIO_IMAGE_BUCKET=images
MINIO_VIDEO_BUCKET=videos
MINIO_DOCUMENT_BUCKET=documents
MINIO_AUDIO_BUCKET=audios
MINIO_OTHER_BUCKET=other
```

Docker Compose maps these values to `Storage__Minio__*` configuration for Media
Service. Never commit `.env` or production credentials.

## Provisioning and startup

Start MinIO and provision Media Service storage:

```bash
docker compose up -d minio minio-init
```

`minio-init` waits for MinIO to become healthy, creates all five buckets
idempotently, creates a dedicated application user, and attaches a policy
limited to those buckets. Media Service does not create buckets at runtime and
will fail startup if its MinIO configuration is missing or invalid.

The MinIO API is available at `http://localhost:9000`; its development console
is available at `http://localhost:9001`.

## Validation and operations

`MinioStorageService` supports streamed upload and download, existence checks,
metadata lookup, and deletion. Before calling MinIO it verifies:

- the stream can be read or written as required;
- the declared upload size is positive and matches seekable streams;
- the extension is normalized and contains only letters or numbers;
- the MIME type matches `IMAGE`, `VIDEO`, `DOCUMENT`, `AUDIO`, or `OTHER`;
- an existing-object operation targets one of the configured buckets.

MinIO SDK exceptions are wrapped as application-owned storage exceptions.
Credentials and object references are not written to logs.

## Health check

`GET /health` checks Media Service's database and all five MinIO buckets.
Storage probing uses lightweight bucket-existence calls with a short timeout;
it never uploads a test object.

Run the isolated MinIO tests:

```bash
dotnet test backend/Services/Media/MediaService.IntegrationTests/MediaService.IntegrationTests.csproj
```
