# Development Data Seed Strategy

Large development datasets are created by
`backend/Tools/Lms.DataSeeder`, not by HTTP endpoints or SQL migrations.
`scripts/seed/run-development-seed.sh` is the guarded user entrypoint and the
Docker Compose `seed` profile keeps the tool opt-in.

## Ownership

- Seeder writes Students through the Student database connection.
- Seeder writes Courses, Lessons and Enrollments through the Course database
  connection.
- `enrollments.student_id` remains a logical cross-database reference; no
  cross-database foreign key is introduced.
- Business services do not reference the seeder project.

The console tool is a development adapter outside the service runtime. It reuses
the physical database contracts because this workload is intended for local
performance/demo data and inserting hundreds of thousands of rows through APIs
would distort both runtime and benchmark results.

## Determinism and idempotency

The generator derives UUIDs and relationship choices from:

```text
random-seed + entity type + deterministic indexes
```

The same options produce the same rows. Multi-row inserts use no-op duplicate
handling, allowing an interrupted run to continue with `--resume`. A different
random seed represents a different dataset and must not be mixed into non-empty
tables.

## Write phases

1. Validate environment, exact database names, migration/table availability,
   current row counts and acquire an advisory lock.
2. Calculate exact expected Lesson and Enrollment totals.
3. Seed Students.
4. Seed Courses.
5. Seed Lessons after their parent Courses.
6. Seed Enrollments after Students and Courses.
7. Validate exact totals, relationship ranges and a cross-database logical
   Student reference.

Each batch is parameterized and transactional. Default batch size is `1,000`;
the tool generates and releases one batch at a time.

## Default scale

- `100,000` Students.
- `100,000` Courses.
- `1-5` Lessons per Course.
- `1-10` enrollments per Student.
- No lesson progress.

See [`../guide/DATA_SEED_GUIDE.md`](../guide/DATA_SEED_GUIDE.md) for commands,
resume/reset behavior and operational troubleshooting.
