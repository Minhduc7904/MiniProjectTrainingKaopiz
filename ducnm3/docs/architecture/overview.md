# Architecture Overview

## Status

The backend uses ASP.NET Core microservices, YARP Gateway, MySQL and MinIO.
Course, Student, Media and Notification are business services. Scheduler is a
platform service with its own API, Worker skeleton and database.

Each service owns its database. Media alone owns MinIO access. Scheduler stores
generic `background_jobs` and `background_job_runs`; it does not query another
service database. Notification retains bulk content, recipient snapshots and
delivery counters in its own batch tables.

Current Scheduler scope is structure-only: health, schema, scaffolded
persistence and a non-running Worker skeleton. CRON parsing, run claiming,
handler execution and cross-service calls remain follow-up work.

See `microservices.md`, `clean-architecture.md` and
`../database/lms-data-model.md` for the detailed boundaries.
