# Notification Snapshot UPSERT Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Allow Notification Worker to insert idempotent snapshot recipients on MySQL without the ambiguous-column failure.

**Architecture:** Preserve the existing bounded multi-row insert and unique `(batch_id, student_id)` idempotency constraint. Use `INSERT IGNORE` so MySQL skips only duplicate recipients and returns the number of rows actually inserted; prove the production repository and migrations work against an isolated MySQL 8.4 Testcontainer.

**Tech Stack:** .NET 10, EF Core, MySqlConnector, MySQL 8.4, NUnit, Testcontainers.MySql.

---

### Task 1: Reproduce and prevent the MySQL UPSERT regression

**Files:**
- Create: `backend/Services/Notification/NotificationService.IntegrationTests/Persistence/NotificationBatchSnapshotIntegrationTests.cs`
- Modify: `backend/Services/Notification/NotificationService.Infrastructure/Persistence/Repositories/EfNotificationBatchRepository.cs:148-153`
- Modify: `docs/tests/notification-service/integration.md`

- [ ] **Step 1: Write the failing production-repository integration test.**

  Create a `NotificationBatchSnapshotIntegrationTests` fixture with an isolated `mysql:8.4` Testcontainer, apply every production Notification migration using `SqlMigrationRunner`, seed a batch in `SNAPSHOTTING`, and call `AppendSnapshotPageAsync` with two fixed student IDs. Assert the method returns `2` and an independent `NotificationDbContext` reads exactly those two IDs as `PENDING`.

- [ ] **Step 2: Verify RED.**

  Run:

  ```bash
  dotnet test backend/Services/Notification/NotificationService.IntegrationTests/NotificationService.IntegrationTests.csproj --filter "FullyQualifiedName~NotificationBatchSnapshotIntegrationTests"
  ```

  Expected: MySQL fails at `ON DUPLICATE KEY UPDATE id = id` with `Column 'id' in field list is ambiguous`.

- [ ] **Step 3: Apply the minimal SQL fix.**

  In `AppendSnapshotPageAsync`, replace the duplicate-update UPSERT with:

  ```sql
  INSERT IGNORE INTO notification_batch_items (...)
  ```

  This makes the return value equal the number of newly inserted rows. Do not change batching, status checks, recipient selection, schema, or message contracts.

- [ ] **Step 4: Verify GREEN and formatting.**

  Run the focused fixture twice, then the exact integration project and format verification:

  ```bash
  dotnet test backend/Services/Notification/NotificationService.IntegrationTests/NotificationService.IntegrationTests.csproj --filter "FullyQualifiedName~NotificationBatchSnapshotIntegrationTests"
  dotnet test backend/Services/Notification/NotificationService.IntegrationTests/NotificationService.IntegrationTests.csproj
  dotnet format backend/Services/Notification/NotificationService.IntegrationTests/NotificationService.IntegrationTests.csproj --verify-no-changes
  ```

- [ ] **Step 5: Document evidence.**

  Add the fixture and its MySQL/Testcontainers behavior to `docs/tests/notification-service/integration.md`.

### Task 2: Recover the failed operational batch

**Files:**
- No tracked-file changes.

- [ ] **Step 1: Rebuild only Notification Worker after the code verification is green.**

  ```bash
  docker compose up -d --build notification-worker
  ```

- [ ] **Step 2: Replay the unchanged fault message.**

  Move the message for batch `6106c006-b2cf-495c-b14b-061d6e4d3db7` from `notification-service--snapshot-notification-batch-v1_error` to `notification-service--snapshot-notification-batch-v1` without editing its payload or correlation ID.

- [ ] **Step 3: Verify state transition.**

  Confirm the batch creates 966 snapshot items and progresses out of `SNAPSHOTTING`; inspect Worker logs and the RabbitMQ error queue for a new fault.

## Self-review

- Scope is one MySQL repository defect plus recovery of the known faulted message.
- The regression test uses production migrations and a Testcontainer, never Docker Compose data.
- No schema, API, or messaging-contract change is included.
