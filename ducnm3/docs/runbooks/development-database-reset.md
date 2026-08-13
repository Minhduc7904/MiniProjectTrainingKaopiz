# Development Database Reset Runbook

Use this runbook only for disposable local development databases when the clean
`V001` baseline changes and old `schema_migrations` checksums must not remain.

## Preconditions

- `.env` exists and contains `ASPNETCORE_ENVIRONMENT=Development`.
- The five database names are exactly `lms_course_db`, `lms_student_db`,
  `lms_media_db`, `lms_notification_db`, and `lms_scheduler_db`.
- No local MySQL data needs to be preserved.
- MinIO data must remain intact.

## Procedure

```bash
scripts/database/reset-development-databases.sh --confirm
docker compose up -d --build
```

The guarded script stops API containers, starts/health-checks MySQL, drops only
the five expected databases, force-recreates `mysql-init`, and leaves
`minio-data` untouched. API startup then applies each service's clean `V001`.

This reset also removes every row created by `Lms.DataSeeder`, because seed data
is written to `lms_student_db` and `lms_course_db` rather than a separate
database or migration. No extra seed cleanup command is required. To recreate
the default dataset after services apply migrations:

```bash
scripts/seed/run-development-seed.sh --confirm
```

## Verification

Each database must contain exactly one `schema_migrations` row with version
`001`. Notification must contain `notification_batches`,
`notification_batch_items`, and `notifications`; Scheduler must contain only
`background_jobs` and `background_job_runs` as business tables.
`students`, `courses`, `lessons`, and `enrollments` must be empty before a new
seed run.

## Failure and recovery

- Missing `--confirm`, non-Development environment, or unexpected database
  names must stop before any drop.
- If reset fails after a drop, rerun the same confirmed script after fixing
  MySQL/Compose. Do not insert/delete migration history rows manually.
- If a V001 DDL statement fails, fix it before shared use, rerun the full local
  reset, and apply all migrations again. MySQL DDL may auto-commit.
- There is no rollback restoring deleted MySQL data. Restore from a backup if
  the precondition that data is disposable was wrong.
