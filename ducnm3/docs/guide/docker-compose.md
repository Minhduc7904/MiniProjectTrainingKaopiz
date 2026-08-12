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
- Course Service, Student Service, Media Service, and Notification Service expose port `8080` only on the internal Docker network.

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

`.env` contains non-secret local development values:

```dotenv
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
Swagger__Enabled=true
```

Do not store passwords, connection strings, API keys, or production secrets in this file. Use a secure deployment secret store when infrastructure is added.

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
