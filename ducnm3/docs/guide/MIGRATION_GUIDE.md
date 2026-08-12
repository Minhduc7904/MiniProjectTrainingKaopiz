# Database First and SQL Migration Guide

## Architecture and source of truth

The project uses MySQL with Database First:

```text
versioned SQL migration
    -> MySQL schema
    -> EF Core scaffold
    -> generated DbContext and persistence models
```

SQL migrations are the only source of truth for schema. Do not use `dotnet ef migrations add`, `dotnet ef database update`, or runtime EF `Database.Migrate()` to manage schema.

Each service owns one database and one migration folder:

```text
Course Service       lms_course_db        Services/Course/CourseService.Infrastructure/Database/Migrations/
Student Service      lms_student_db       Services/Student/StudentService.Infrastructure/Database/Migrations/
Media Service        lms_media_db         Services/Media/MediaService.Infrastructure/Database/Migrations/
Notification Service lms_notification_db  Services/Notification/NotificationService.Infrastructure/Database/Migrations/
```

There are no foreign keys across service databases. Store external IDs such as `student_id` as scalar values and validate them through service contracts when needed.

## Credentials and environment variables

`.env` is local-only and ignored by Git. Start from `.env.example`:

```bash
cp .env.example .env
```

Set real local passwords in `.env`. Never place passwords, root credentials, or connection strings in source code or `appsettings.json`.

Quote connection-string values in `.env` because `User ID` contains a space and the file is also loaded by shell-based automation.

Each API receives only its own `ConnectionStrings__Database` environment variable. Docker Compose forwards the matching runtime connection string from `.env`. Manual automation uses:

- `COURSE_DB_LOCAL_CONNECTION_STRING`
- `STUDENT_DB_LOCAL_CONNECTION_STRING`
- `MEDIA_DB_LOCAL_CONNECTION_STRING`
- `NOTIFICATION_DB_LOCAL_CONNECTION_STRING`

The local variants use `Server=localhost` for host-side migration and scaffold commands. The Docker runtime variants use `Server=mysql`.

## Migration history and startup behavior

Before applying service migrations, the application creates this technical table in its own database:

```text
schema_migrations
├── version
├── name
├── applied_at
└── checksum
```

At startup, every API:

1. Waits for MySQL and the one-shot `mysql-init` container.
2. Acquires a MySQL migration lock for its own database.
3. Creates `schema_migrations` if it does not exist.
4. Reads SQL files in version order.
5. Verifies checksums of previously applied migrations.
6. Applies each pending migration, records its history row, and logs the version.
7. Fails startup when a migration, checksum validation, or history write fails.

Errors are never silently ignored. MySQL DDL can implicitly commit, so a failed DDL migration may need a corrective forward migration or manual development reset.

## Migration naming convention

Use:

```text
V<zero-padded-version>__<lowercase-description>.sql
```

Examples:

```text
V001__initialize.sql
V002__create_courses.sql
V003__add_course_status_index.sql
V004__add_description_markdown.sql
```

Rules:

- Versions are unique and increase within one service.
- Never edit an applied migration. Its checksum is validated at startup.
- Use lower-case letters, numbers, hyphens, and underscores after `__`.
- One migration should make one coherent schema change.

## Create and apply a migration

Create the SQL file in the owning service migration folder, for example:

```bash
touch backend/Services/Course/CourseService.Infrastructure/Database/Migrations/V002__create_courses.sql
```

Write schema SQL there. Then run the service migration locally:

```bash
set -a
. ./.env
set +a
sh backend/database/tools/migrate.sh course
```

The same migration runs automatically before the API starts in Docker.

## Add schema changes

### Add a table

Create a new migration:

```sql
CREATE TABLE courses (
    id CHAR(36) NOT NULL,
    name VARCHAR(200) NOT NULL,
    created_at DATETIME NOT NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB;
```

### Add a column

Create a later migration; do not edit the table-creation migration after it is applied:

```sql
ALTER TABLE courses
    ADD COLUMN description_markdown TEXT NULL;
```

### Add an index

Use a dedicated migration and a descriptive name:

```sql
CREATE INDEX ix_courses_created_at ON courses (created_at);
```

## Rollback and failed migrations

Production uses forward-only migrations. Prefer a new corrective migration over rollback.

For a failed migration:

1. Stop and inspect the logged migration version and MySQL error.
2. Check whether MySQL applied any DDL before the failure.
3. In shared or production environments, create a new forward migration that repairs the schema.
4. In a disposable local development database, reset the database and rerun migrations if appropriate.

Do not delete or alter a row in `schema_migrations` merely to rerun a migration unless the actual schema has been reset to match.

## Reset a development database

This removes all local MySQL data:

```bash
docker compose down -v
docker compose up -d --build
```

Docker Compose recreates the four databases, bootstrap users, migration history, and applies all migrations.

## EF Core Database First scaffold

Run scaffold only after SQL migrations succeed. Scaffold is a development action, never a production startup action.

```bash
set -a
. ./.env
set +a
sh backend/database/tools/scaffold.sh course
```

Replace `course` with `student`, `media`, or `notification`.

The command uses:

```text
dotnet ef dbcontext scaffold
Pomelo.EntityFrameworkCore.MySql
--no-onconfiguring
--no-build
--force
```

Automation chỉ chọn bảng nghiệp vụ của service; không scaffold technical table `schema_migrations`.
`--no-build` cho phép scaffold lại ngay cả khi generated source hiện tại chưa khớp schema; luôn chạy `dotnet build` ngay sau scaffold.

Generated code belongs only in the matching service Infrastructure project:

```text
Infrastructure/
└── Persistence/
    ├── <Service>DbContext.cs
    └── Scaffolded/
```

Never scaffold another service database. For example, Student Service never scaffolds `lms_course_db`.

## Re-scaffold safely

Scaffolded `DbContext` and entities are generated code:

- Do not put business logic in generated files.
- Use partial classes for generated model extensions.
- Use mappers to convert persistence models to Domain entities.
- Keep use cases and interfaces in Application.
- Rerun scaffold with `--force` after every approved schema migration.

Review generated diffs after each scaffold to ensure only the owning service changed.

## Docker workflow

```bash
docker compose up -d --build
```

Startup dependency:

```text
mysql healthy
    -> mysql-init creates databases and users
    -> service applies its SQL migrations
    -> service starts
```

In production, use the same ordering but inject real credentials through the deployment secret store. Do not run scaffold in production.

## Daily developer workflow

1. Update `.env` locally from `.env.example`.
2. Start MySQL and services with Docker Compose.
3. Add a versioned SQL migration for schema changes.
4. Apply and inspect the migration.
5. Scaffold only the owning service database.
6. Implement persistence mapping in Infrastructure and business behavior in Domain/Application.
7. Run build and tests.
8. Commit SQL migrations, generated scaffold files when applicable, `.env.example`, and guide updates; never commit `.env`.
