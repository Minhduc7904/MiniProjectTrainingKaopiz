# Documentation

This directory contains the LMS microservices plan, organized by subject. Treat its technical choices as proposals until the project explicitly adopts them.

## Start here

- [Project overview](overview.md)
- [Architecture proposal](architecture/microservices.md)
- [Technology stack](development/tech-stack.md)
- [Five-day implementation plan](plan/)

## Architecture

- `architecture/microservices.md`: service boundaries, database ownership, and responsibilities.
- `architecture/clean-architecture.md`: clean architecture and service folder structures.
- `architecture/rich-content-and-media.md`: Media Service ownership, Markdown content, and media usage lifecycle.
- `architecture/uml.md`: required UML diagrams.
- `architecture/conclusion.md`: final architecture summary.

## Business flows

- `business-flows/`: course management, media usage, enrollment, single/bulk notification, inbox, and notification media content.

## API and data

- `api/minimum-api.md`: media upload, single/bulk notification, inbox, and minimum endpoint set.
- `api/error-handling-observability.md`: error contract, logging, and observability.
- `database/lms-data-model.md`: LMS, multi-media, notification-job, and student-inbox entities.

## Development and operations

- `development/`: stack, MinIO, Docker, performance, delivery, and preparation guides.
- `runbooks/notification-batch.md`: batch, retry, idempotency, and failure handling.
- `runbooks/demo-script.md`: 30-minute demo procedure.

## Daily plans

- `plan/day-01-foundation-docker-clean-architecture.md`
- `plan/day-02-lms-core-minio-n-1.md`
- `plan/day-03-batch-retry-idempotency.md`
- `plan/day-04-performance-day.md`
- `plan/day-05-error-handling-test-slide-demo.md`

Update the relevant document in the same change whenever implementation alters a documented behavior or decision.
