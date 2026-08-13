# Scheduler Service Unit Tests

## Project

`backend/Services/Scheduler/SchedulerService.UnitTests/SchedulerService.UnitTests.csproj`

## `CheckAsyncPropagatesRequestCancellation`

- Source: `Health/SchedulerDatabaseHealthProbeTests.cs`.
- Unit under test: `SchedulerDatabaseHealthProbe.CheckAsync`.
- Setup: an unreachable local MySQL connection string and `NullLogger`; the request cancellation token is cancelled before invocation.
- Input/dependency state: cancelled token, with no shared development database access.
- Expected result: `OperationCanceledException` is propagated instead of being converted to an unhealthy probe result.
- Pass condition: NUnit `Assert.ThrowsAsync<OperationCanceledException>` succeeds.

Database-unavailable response mapping is the shared
`MapDatabaseHealthEndpoint` behavior documented under
`docs/tests/shared-presentation/component.md`. There are no Scheduler execution
tests yet because Worker polling, CRON parsing, run claiming and service calls
are not implemented.

Run:

```bash
dotnet test backend/Services/Scheduler/SchedulerService.UnitTests/SchedulerService.UnitTests.csproj
```
