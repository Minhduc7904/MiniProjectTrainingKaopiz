# Notification Success Sender Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Notification delivery succeed by default while preserving the sender port for a future email or SMS provider.

**Architecture:** Add a stateless Infrastructure implementation of `INotificationSender` that completes successfully after observing cancellation. Register it as the sole default sender in `AddNotificationInfrastructure`; retain `FakeNotificationSender` as an explicit test/dev adapter, but do not resolve it from production DI.

**Tech Stack:** .NET 10, Microsoft.Extensions.DependencyInjection, NUnit.

---

### Task 1: Prove default delivery semantics

**Files:**
- Create: `backend/Services/Notification/NotificationService.UnitTests/Infrastructure/Services/Sending/SuccessfulNotificationSenderTests.cs`
- Modify: `docs/tests/notification-service/unit.md`

- [ ] **Step 1: Write the failing sender test.**

  Add `SendAsync_AnyRecipient_CompletesWithoutFailure` using fixed UUID `a23f9390-b3ae-57a5-a74a-6f3e69226a8e`. Construct `SuccessfulNotificationSender`, await `SendAsync(studentId, 1, CancellationToken.None)`, and assert that no exception occurs.

- [ ] **Step 2: Verify RED.**

  Run:

  ```bash
  dotnet test backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj --filter "FullyQualifiedName~SuccessfulNotificationSenderTests"
  ```

  Expected: compile failure because `SuccessfulNotificationSender` does not exist.

- [ ] **Step 3: Implement the smallest sender adapter.**

  Create `SuccessfulNotificationSender` in Infrastructure. Its `SendAsync` calls `cancellationToken.ThrowIfCancellationRequested()` and returns `Task.CompletedTask`; it does not inspect `studentId` or `attempt` and performs no external I/O.

- [ ] **Step 4: Verify GREEN.**

  Re-run the focused command. Expected: one passing test.

### Task 2: Switch the production composition root

**Files:**
- Create: `backend/Services/Notification/NotificationService.UnitTests/Infrastructure/NotificationInfrastructureDependencyInjectionTests.cs`
- Modify: `backend/Services/Notification/NotificationService.Infrastructure/DependencyInjection.cs`
- Modify: `docs/architecture/backend/notification-service/details/infrastructure.md`
- Modify: `docs/api/notification-service/endpoints/post-notification-batches.md`
- Modify: `docs/tests/notification-service/unit.md`

- [ ] **Step 1: Write the failing DI registration test.**

  Build in-memory configuration with `ServiceEndpoints:student = http://student.test/`, instantiate `ServiceCollection`, call `AddNotificationInfrastructure(configuration, "Server=localhost;Database=notification;User Id=user;Password=password;")`, then resolve `INotificationSender`. Assert it is `SuccessfulNotificationSender` and is not `FakeNotificationSender`.

- [ ] **Step 2: Verify RED.**

  Run:

  ```bash
  dotnet test backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj --filter "FullyQualifiedName~NotificationInfrastructureDependencyInjectionTests"
  ```

  Expected: assertion fails because DI currently resolves `FakeNotificationSender`.

- [ ] **Step 3: Switch the registration.**

  Replace `services.AddSingleton<INotificationSender, FakeNotificationSender>();` with `services.AddSingleton<INotificationSender, SuccessfulNotificationSender>();`. Do not change `INotificationSender`, dispatch retry state transitions, API contracts, database schema, or `FakeNotificationSender`.

- [ ] **Step 4: Update behavior documentation.**

  State that the default adapter records no external delivery and treats the inbox write as successful. State that email/SMS providers replace the DI binding later, while Fake remains only for explicit fault-path testing.

- [ ] **Step 5: Verify regression scope.**

  Run:

  ```bash
  dotnet test backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj --filter "FullyQualifiedName~(SuccessfulNotificationSenderTests|NotificationInfrastructureDependencyInjectionTests|FakeNotificationSenderTests)"
  dotnet test backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj
  dotnet format backend/Services/Notification/NotificationService.UnitTests/NotificationService.UnitTests.csproj --verify-no-changes
  git diff --check
  ```

  Expected: sender and DI tests pass, the full Notification unit project passes, and no whitespace errors are reported. If format reports unrelated existing WIP files, report those separately without modifying them.

## Self-review

- The sender port and Fake adapter remain available; only the production DI binding changes.
- No retry, API, messaging, migration, or schema behavior is changed.
- Both the sender contract and the composition-root binding have focused regression tests.
