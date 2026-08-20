# Course và Lesson Markdown Update Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tạo Course, cập nhật partial Course/Lesson và đồng bộ chính xác media usage của hai Markdown field qua Media worker.

**Architecture:** Course Service nhận optional JSON fields, đọc snapshot, cập nhật phần được gửi và tính diff tham chiếu Markdown. Một command Media version mới mang `added`/`removed`; worker Media tạo hoặc soft-delete usage idempotently theo owner type. Không thay đổi schema.

**Tech Stack:** ASP.NET Core Minimal API, .NET 10, EF Core/MySQL, MassTransit command worker, NUnit/TestServer.

---

## File structure

- Course API: thêm contracts/endpoint cho CreateCourse, UpdateCourse và UpdateLesson; mở rộng `ApiRoutes.cs` và `Program.cs`.
- Course Application: thêm `ContentMediaUsageDiff`, command handlers và repository port cho snapshot/create/update.
- Course Infrastructure: triển khai port bằng `CourseDbContext` trong transaction và map conflict unique index.
- Media contracts/application/worker: thêm command sync diff, `COURSE_DESCRIPTION`, repository methods để ensure/remove theo owner và consumer.
- Tests: unit test Application và Media handler; component test TestServer cho HTTP binding/envelope; cập nhật catalogs, API docs, business flows và Postman.

### Task 1: Khóa diff Markdown và message contract

**Files:**
- Create: `backend/Services/Course/CourseService.Application/Services/Content/ContentMediaUsageDiff.cs`
- Modify: `backend/Services/Media/MediaService.Contracts/Messaging/RegisterNotificationMediaUsageV1.cs`
- Modify: `backend/Services/Media/MediaService.Domain/Constants/MediaOwnerTypes.cs`
- Test: `backend/Services/Course/CourseService.UnitTests/Application/Content/ContentMediaUsageDiffTests.cs`

- [ ] **Step 1: Write the failing diff test**

```csharp
[Test]
public void Create_OldAndNewMarkdown_ReturnsOnlyAddedAndRemovedReferences()
{
    var oldMarkdown = $"[old]({ApiRoutes.Media.ContentPublicPath(OldMediaId)})";
    var newMarkdown = $"![new]({ApiRoutes.Media.ContentPublicPath(NewMediaId)})";

    var result = ContentMediaUsageDiff.Create(oldMarkdown, newMarkdown);

    Assert.That(result.Added, Is.EqualTo([
        new NotificationMediaUsageReferenceV1(NewMediaId, NotificationMediaUsageTypes.Embed, 0)]));
    Assert.That(result.Removed, Is.EqualTo([
        new NotificationMediaUsageReferenceV1(OldMediaId, NotificationMediaUsageTypes.Attachment, 0)]));
}
```

- [ ] **Step 2: Run focused test and verify RED**

Run: `dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj --filter "FullyQualifiedName~ContentMediaUsageDiffTests"`

Expected: FAIL because `ContentMediaUsageDiff` does not exist.

- [ ] **Step 3: Implement the minimal diff and sync contract**

```csharp
public sealed record ContentMediaUsageDiff(
    IReadOnlyList<NotificationMediaUsageReferenceV1> Added,
    IReadOnlyList<NotificationMediaUsageReferenceV1> Removed)
{
    public static ContentMediaUsageDiff Create(string? before, string? after) { /* set difference by MediaId + UsageType */ }
}

public sealed record SynchronizeCourseMediaUsageV1(
    Guid OwnerId, string OwnerType, Guid CreatedBy,
    IReadOnlyList<NotificationMediaUsageReferenceV1> Added,
    IReadOnlyList<NotificationMediaUsageReferenceV1> Removed) : ICommand;
```

Add `CourseDescription = "COURSE_DESCRIPTION"` to `MediaOwnerTypes`.

- [ ] **Step 4: Run focused test and verify GREEN**

Run: `dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj --filter "FullyQualifiedName~ContentMediaUsageDiffTests"`

Expected: PASS.

### Task 2: Media worker synchronizes added and removed usages

**Files:**
- Create: `backend/Services/Media/MediaService.Application/UseCases/MediaUsages/SynchronizeCourseContent/SynchronizeCourseContentMediaUsagesHandler.cs`
- Modify: `backend/Services/Media/MediaService.Application/Repositories/IMediaUsageRepository.cs`
- Modify: `backend/Services/Media/MediaService.Infrastructure/Persistence/Repositories/EfMediaUsageRepository.cs`
- Modify: `backend/Services/Media/MediaService.Application/DependencyInjection.cs`
- Modify: `backend/Services/Media/MediaService.Worker/Consumers/MediaUsage/RegisterNotificationMediaUsageConsumer.cs`
- Modify: `backend/Services/Media/MediaService.Worker/Program.cs`
- Test: `backend/Services/Media/MediaService.UnitTests/Application/SynchronizeCourseContentMediaUsagesHandlerTests.cs`

- [ ] **Step 1: Write failing Media handler tests**

```csharp
[Test]
public async Task HandleAsync_MixedDiff_EnsuresAddedAndRemovesOnlyRequestedOwner()
{
    await handler.HandleAsync(new SynchronizeCourseMediaUsageV1(
        OwnerId, MediaOwnerTypes.LessonContent, AdminId,
        [new(NewMediaId, NotificationMediaUsageTypes.Embed, 0)],
        [new(OldMediaId, NotificationMediaUsageTypes.Attachment, 0)]), Token);

    Assert.That(repository.Ensured.Single().MediaId, Is.EqualTo(NewMediaId));
    Assert.That(repository.Removed, Is.EqualTo([(OwnerId, MediaOwnerTypes.LessonContent, OldMediaId, NotificationMediaUsageTypes.Attachment)]));
}
```

- [ ] **Step 2: Run RED**

Run: `dotnet test backend/Services/Media/MediaService.UnitTests/MediaService.UnitTests.csproj --filter "FullyQualifiedName~SynchronizeCourseContentMediaUsagesHandlerTests"`

Expected: FAIL because the command handler and repository members do not exist.

- [ ] **Step 3: Implement worker behavior**

Add `SynchronizeCourseContentMediaUsagesHandler.HandleAsync`, validate owner and actor, map additions to `CreateMediaUsageRecord`, call ensure only for additions and `RemoveCourseContentMediaAsync` for removals. Repository removal filters exactly `(COURSE, ownerType, ownerId, mediaId, usageType)` and soft-deletes all matching active rows; it updates `is_draft` only when the media has no active usages. Register one `IConsumer<SynchronizeCourseMediaUsageV1>` with the existing entity-framework outbox configuration.

- [ ] **Step 4: Run GREEN and idempotency case**

Run: `dotnet test backend/Services/Media/MediaService.UnitTests/MediaService.UnitTests.csproj --filter "FullyQualifiedName~SynchronizeCourseContentMediaUsagesHandlerTests"`

Expected: PASS, including repeated remove with no active record.

### Task 3: Course command persistence and handlers

**Files:**
- Modify: `backend/Services/Course/CourseService.Application/Repositories/ILessonCommandRepository.cs`
- Create: `backend/Services/Course/CourseService.Application/Repositories/ICourseCommandRepository.cs`
- Create: `backend/Services/Course/CourseService.Application/UseCases/Courses/Create/CreateCourseHandler.cs`
- Create: `backend/Services/Course/CourseService.Application/UseCases/Courses/Update/UpdateCourseHandler.cs`
- Create: `backend/Services/Course/CourseService.Application/UseCases/Lessons/Update/UpdateLessonHandler.cs`
- Modify: `backend/Services/Course/CourseService.Application/DependencyInjection.cs`
- Modify: `backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/EfLessonCommandRepository.cs`
- Create: `backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/EfCourseCommandRepository.cs`
- Modify: `backend/Services/Course/CourseService.Infrastructure/DependencyInjection.cs`
- Test: `backend/Services/Course/CourseService.UnitTests/Application/Courses/UpdateCourseHandlerTests.cs`
- Test: `backend/Services/Course/CourseService.UnitTests/Application/Lessons/UpdateLessonHandlerTests.cs`

- [ ] **Step 1: Write failing partial update tests**

```csharp
[Test]
public async Task HandleAsync_DescriptionIsAbsent_KeepsDescriptionAndDoesNotSendMediaCommand()
{
    repository.Course = ExistingCourse(descriptionMarkdown: ExistingMarkdown);
    await handler.HandleAsync(new UpdateCourseCommand(CourseId, Name: "New", DescriptionMarkdown: OptionalField<string?>.Absent, Status: OptionalField<string?>.Absent, AdminId), Token);
    Assert.That(repository.SavedCourse!.DescriptionMarkdown, Is.EqualTo(ExistingMarkdown));
    Assert.That(sender.Messages, Is.Empty);
}
```

- [ ] **Step 2: Run RED**

Run: `dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj --filter "FullyQualifiedName~UpdateCourseHandlerTests|FullyQualifiedName~UpdateLessonHandlerTests"`

Expected: FAIL because optional request fields and handlers do not exist.

- [ ] **Step 3: Implement minimal application and EF persistence**

Use `OptionalField<T>(bool IsPresent, T? Value)` for request-to-command binding. Course/Lesson handler rejects an all-absent command, preserves absent properties, normalizes present empty Markdown to `null`, computes diff only for a present Markdown field, persists updated snapshot, then sends `SynchronizeCourseMediaUsageV1` only when the diff is non-empty. EF uses one save operation and converts MySQL duplicate key 1062 for `(course_id, display_order)` to the existing conflict exception.

- [ ] **Step 4: Run GREEN**

Run: `dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj --filter "FullyQualifiedName~UpdateCourseHandlerTests|FullyQualifiedName~UpdateLessonHandlerTests"`

Expected: PASS.

### Task 4: HTTP contracts, routes and endpoints

**Files:**
- Modify: `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs`
- Create: `backend/Services/Course/CourseService.Api/Contracts/Courses/CreateCourseRequest.cs`
- Create: `backend/Services/Course/CourseService.Api/Contracts/Courses/UpdateCourseRequest.cs`
- Create: `backend/Services/Course/CourseService.Api/Contracts/Courses/UpdateLessonRequest.cs`
- Create: `backend/Services/Course/CourseService.Api/Endpoints/Courses/CreateCourse/CreateCourseEndpoint.cs`
- Create: `backend/Services/Course/CourseService.Api/Endpoints/Courses/UpdateCourse/UpdateCourseEndpoint.cs`
- Create: `backend/Services/Course/CourseService.Api/Endpoints/Courses/UpdateLesson/UpdateLessonEndpoint.cs`
- Modify: `backend/Services/Course/CourseService.Api/Program.cs`
- Test: `backend/Services/Course/CourseService.ComponentTests/Endpoints/CourseCommandEndpointsComponentTests.cs`

- [ ] **Step 1: Write failing TestServer contract tests**

```csharp
[Test]
public async Task Put_CourseDescriptionNull_ReturnsUpdatedEnvelope()
{
    using var response = await client.PutAsJsonAsync(ApiRoutes.Courses.ByIdServicePath(CourseId), new { descriptionMarkdown = (string?)null }, Token);
    var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CourseResponse>>(Token);
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    Assert.That(envelope!.Data.DescriptionMarkdown, Is.Null);
}
```

- [ ] **Step 2: Run RED**

Run: `dotnet test backend/Services/Course/CourseService.ComponentTests/CourseService.ComponentTests.csproj --filter "FullyQualifiedName~CourseCommandEndpointsComponentTests"`

Expected: FAIL because routes/endpoints are absent.

- [ ] **Step 3: Implement endpoint mapping**

Define `ByIdTemplate`, `LessonByIdTemplate` and service/public path builders. Bind requests with nullable properties plus `JsonExtensionData`/custom presence tracking so absent and `null` remain distinguishable. Map API routes using `MapPost`/`MapPut`, actor headers, `ApiResponseFactory.Success`, `Accepts<T>` and `Produces` metadata; register all mappings in `Program.cs`.

- [ ] **Step 4: Run GREEN**

Run: `dotnet test backend/Services/Course/CourseService.ComponentTests/CourseService.ComponentTests.csproj --filter "FullyQualifiedName~CourseCommandEndpointsComponentTests"`

Expected: PASS for create, missing markdown preservation, null/rỗng clearing, invalid empty request and safe not-found/conflict envelopes.

### Task 5: Documentation, Postman and full verification

**Files:**
- Create: `docs/api/course-service/endpoints/put-course.md`
- Create: `docs/api/course-service/endpoints/put-course-lesson.md`
- Modify: `docs/api/course-service/endpoints/post-courses.md`
- Create: `docs/business-flows/course-learning/put-course.md`
- Create: `docs/business-flows/course-learning/put-course-lesson.md`
- Modify: `docs/business-flows/course-learning/course-management.md`
- Modify: `docs/api/course-service/README.md`
- Modify: `postman/MiniProjectKaopiz.postman_collection.json`
- Modify: `docs/tests/course-service/unit.md`
- Modify: `docs/tests/course-service/component.md`
- Modify: `docs/tests/media-service/unit.md`

- [ ] **Step 1: Update artifacts to the final contract**

Document that PUT is partial by product contract, absence preserves a field, and `null`/empty Markdown clears it. Document worker eventual consistency, owner types, idempotency and all result/error codes. Add public Gateway requests with variables and JSON tests for `POST`, both `PUT` routes and clear/preserve cases.

- [ ] **Step 2: Verify JSON and focused projects**

Run:
`jq empty postman/MiniProjectKaopiz.postman_collection.json`

`dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj`

`dotnet test backend/Services/Course/CourseService.ComponentTests/CourseService.ComponentTests.csproj`

`dotnet test backend/Services/Media/MediaService.UnitTests/MediaService.UnitTests.csproj`

Expected: all pass.

- [ ] **Step 3: Run repository quality checks**

Run:
`dotnet build backend/Lms.sln -m:1`

`dotnet test backend/Lms.sln -m:1`

`dotnet format backend/Lms.sln --verify-no-changes`

Expected: all commands exit 0; report any pre-existing failures separately.
