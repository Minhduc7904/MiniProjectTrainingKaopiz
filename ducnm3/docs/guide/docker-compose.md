# Docker Compose and Shared Swagger Guide

## Prerequisites

- Docker Engine and Docker Compose plugin are running.
- Work from the `ducnm3/` directory, which contains `docker-compose.yml` and `.env`.

## Start the stack

```bash
docker compose up --build
```

Run detached:

```bash
docker compose up -d --build
```

Stop containers and the network:

```bash
docker compose down
```

## Service ports

- API Gateway and shared Swagger UI: `http://localhost:5100`
- Course Service: `http://localhost:5101`
- Student Service: `http://localhost:5102`
- Media Service: `http://localhost:5103`
- Notification Service: `http://localhost:5104`
- MinIO API: `http://localhost:9000`
- MinIO console: `http://localhost:9001`

Gateway routes external requests by service prefix:

- `/course/{path}` → Course Service
- `/student/{path}` → Student Service
- `/media/{path}` → Media Service
- `/notification/{path}` → Notification Service

For example, Course health is available through `http://localhost:5100/course/health`.

## Shared Swagger UI

`api-gateway` serves a single NSwag UI at `http://localhost:5100/swagger`. Use the document selector to load the APIs for one service at a time:

- Course Service
- Student Service
- Media Service
- Notification Service

The Gateway proxies each document through the same origin:

- `/course/swagger/v1/swagger.json`
- `/student/swagger/v1/swagger.json`
- `/media/swagger/v1/swagger.json`
- `/notification/swagger/v1/swagger.json`

No browser-to-service CORS configuration is needed because the UI and documents are served through the Gateway.

## Environment configuration

`.env` contains local development configuration and credentials:

```dotenv
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
Swagger__Enabled=true
MINIO_ROOT_USER=minio-root-user
MINIO_ROOT_PASSWORD=replace-with-a-long-root-secret
MINIO_APP_ACCESS_KEY=media-storage-app
MINIO_APP_SECRET_KEY=replace-with-a-long-app-secret
MINIO_IMAGE_BUCKET=images
MINIO_VIDEO_BUCKET=videos
MINIO_DOCUMENT_BUCKET=documents
MINIO_AUDIO_BUCKET=audios
MINIO_OTHER_BUCKET=other
```

`.env` is ignored by Git. Never put production credentials in `.env.example`;
use a deployment secret store outside local development.

## MinIO provisioning

`minio` stores object data in the persistent `minio-data` volume. `minio-init`
waits for the MinIO health endpoint, creates the five Media Service buckets,
and provisions a least-privilege application user. The initialization script is
safe to run repeatedly and the Media Service waits for it to finish.

Run only the storage dependencies:

```bash
docker compose up -d minio minio-init
```

Remove containers while preserving data:

```bash
docker compose down
```

Remove containers and both MySQL/MinIO development volumes:

```bash
docker compose down -v
```

## Troubleshooting

Check container status:

```bash
docker compose ps
```

Inspect all logs:

```bash
docker compose logs --follow
```

Rebuild one service:

```bash
docker compose build media-service
docker compose up -d media-service
```
