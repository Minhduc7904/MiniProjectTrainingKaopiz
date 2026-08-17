# Architecture Documentation Restructure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (\`- [ ]\`) syntax for tracking.

**Goal:** Tách và cập nhật tài liệu kiến trúc theo Backend/Frontend, với `architecture.md` tổng quan và tài liệu layer chi tiết cho mỗi service và BuildingBlocks.

**Architecture:** \`docs/architecture/\` trở thành mục lục mỏng, hai miền \`backend/\` và \`frontend/\`; Backend có overview/shared docs, sáu tài liệu BuildingBlocks và năm service docs. Nội dung cũ được hợp nhất hoặc tách theo ownership, sau đó link cũ được thay bằng đường dẫn mới.

**Tech Stack:** Markdown, Mermaid, ASP.NET Core Clean Architecture, React, Redux Toolkit, Axios, YARP, MassTransit, MySQL, MinIO.

---

## File structure

| Path | Responsibility |
| --- | --- |
| \`docs/architecture/README.md\` | Mục lục kiến trúc và đường dẫn đọc theo Backend/Frontend. |
| \`docs/architecture/backend/overview.md\` | Ranh giới hệ thống, Gateway, năm service, shared infrastructure, ownership. |
| \`docs/architecture/backend/shared/*.md\` | Quy tắc Clean Architecture, communication và message contract. |
| \`docs/architecture/backend/building-blocks/*.md\` | Kiến trúc và cách dùng từng nhóm shared library Backend. |
| \`docs/architecture/backend/<service>/architecture.md\` | Ownership, flow, layer, data boundary, current/future state của đúng một service. |
| \`docs/architecture/backend/<service>/details/*.md\` | Tài liệu layer/runtime chi tiết, chỉ tạo cho project tồn tại trong source. |
| \`docs/architecture/backend/uml.md\` | Sơ đồ Backend đã cập nhật link/nội dung. |
| \`docs/architecture/frontend/architecture.md\` | Kiến trúc React SPA và Gateway boundary. |
| \`docs/README.md\`, \`docs/development/{setup,tech-stack}.md\` | Link tới đường dẫn kiến trúc mới. |

### Task 1: Audit source of truth and map claims

**Files:**
- Read: \`backend/**/{*.csproj,Program.cs}\`, \`backend/Gateway/Lms.ApiGateway/Program.cs\`
- Read: \`frontend/lms-web/src/{app,api,features,hooks,components,pages}/**\`
- Read: \`docs/api/**\`, \`docs/database/**\`, \`docker-compose*.yml\`
- Modify: \`docs/superpowers/plans/2026-08-17-architecture-documentation-restructure.md\`

- [ ] **Step 1: Record Backend service projects and runtime roles**

Run:

\`\`\`bash
find backend/Services -maxdepth 3 -name '*.csproj' | sort
\`\`\`

Expected: Course, Student, Media, Notification and Scheduler project boundaries, including Worker projects where present.

- [ ] **Step 2: Verify transport, database and storage claims**

Run:

\`\`\`bash
rg -n 'RabbitMQ|MassTransit|MinIO|ConnectionStrings|Map' backend docs/api docs/database
\`\`\`

Expected: evidence for the communication, ownership and health-check statements used by the new docs.

- [ ] **Step 3: Classify each claim**

Write only source-backed behavior in a service doc's **Đã triển khai hiện tại** section. Move an unimplemented Cron, polling, claim, handler or integration behavior into **Định hướng/chưa triển khai**.

- [ ] **Step 4: Confirm the audit has no code changes**

Run:

\`\`\`bash
git diff -- backend frontend
\`\`\`

Expected: no output.

### Task 2: Create the Backend documentation hierarchy

**Files:**
- Create: \`docs/architecture/README.md\`
- Create: \`docs/architecture/backend/overview.md\`
- Create: \`docs/architecture/backend/shared/clean-architecture.md\`
- Create: \`docs/architecture/backend/shared/service-communication.md\`
- Create: \`docs/architecture/backend/shared/message-contract-template.md\`
- Create: \`docs/architecture/backend/uml.md\`
- Delete: \`docs/architecture/{overview,microservices,clean-architecture,service-communication,message-contract-template,uml,conclusion}.md\`

- [ ] **Step 1: Create the architecture index and Backend overview**

Write \`README.md\` with Backend and Frontend navigation. Write \`backend/overview.md\` with a Mermaid component diagram covering Client → Gateway → five services, five isolated MySQL databases, RabbitMQ and MinIO. State that Media alone owns MinIO and cross-service database access is prohibited.

- [ ] **Step 2: Move shared Backend rules without duplicating service behavior**

Put dependency rules, BuildingBlocks and testing boundaries in \`shared/clean-architecture.md\`; HTTP/RabbitMQ/retry/idempotency rules in \`shared/service-communication.md\`; versioning/ownership checklist in \`shared/message-contract-template.md\`.

- [ ] **Step 3: Update Backend UML**

Keep the component, Course and Notification diagrams in \`backend/uml.md\`. Make future-only Scheduler behavior visibly dashed/labeled and link related service docs.

- [ ] **Step 4: Remove superseded root Backend files**

Delete only the listed files after their content and inbound links have been preserved in the destination files.

### Task 3: Write the BuildingBlocks architecture documentation

**Files:**
- Create: \`docs/architecture/backend/building-blocks/architecture.md\`
- Create: \`docs/architecture/backend/building-blocks/details/{contracts,presentation,http,messaging,database-migration,testing}.md\`
- Modify: \`docs/architecture/README.md\`
- Read: \`backend/BuildingBlocks/**/*.{cs,csproj}\`

- [ ] **Step 1: Map actual project references and public extension points**

Run:

\`\`\`bash
find backend/BuildingBlocks -name '*.csproj' -o -name '*.cs' | sort
rg -n 'ProjectReference|public (static )?(class|interface)|AddLms|UseLms|Map[A-Z]' backend/BuildingBlocks
\`\`\`

Expected: contracts, presentation, HTTP, messaging, database migration and test boundaries are documented from source rather than assumed from service docs.

- [ ] **Step 2: Write the BuildingBlocks index and dependency graph**

Write \`architecture.md\` with a Mermaid graph where `Presentation` and `Messaging` depend on `Contracts`; `Messaging` also depends on `Messaging.Abstractions`; `Http` depends on `Contracts`; `DatabaseMigration` is independent. Label test projects as validation-only. Link the six focused documents from this index and add the index to \`docs/architecture/README.md\`.

- [ ] **Step 3: Write Contracts and Presentation documentation**

In \`contracts.md\`, explain API contracts/constants, health probes and Student query DTO without placing domain rules in a shared contract. In \`presentation.md\`, document correlation and exception middleware ordering, response factory, CORS, `/` and database health endpoint, and Gateway Swagger aggregation. Include source-level method signatures or registration snippets and a troubleshooting table in both files.

- [ ] **Step 4: Write HTTP and Messaging documentation**

In \`http.md\`, explain `AddServiceQueryClient`, `CorrelationIdDelegatingHandler`, configured endpoint validation and resilience. In \`messaging.md\`, keep `ICommand`/`IIntegrationEvent`/sender-publisher contracts separate from the MassTransit RabbitMQ adapter, consumer registration, endpoint naming, retry and health probe. Include Mermaid flow, use snippets and troubleshooting for both documents.

- [ ] **Step 5: Write Database Migration and Testing documentation**

In \`database-migration.md\`, document `SqlMigrationRunner`, service-scoped connection/migration directory, advisory lock, checksum/history behavior and failure recovery. In \`testing.md\`, distinguish communication unit tests, Presentation TestServer tests and RabbitMQ Testcontainers tests; state that test projects are not runtime dependencies. Include commands, expected prerequisites and troubleshooting.

- [ ] **Step 6: Classify actual versus future behavior**

For every BuildingBlocks document, add **Đã triển khai hiện tại** and **Định hướng/chưa triển khai**. Do not claim standardized inbox/outbox, distributed tracing exporter, API versioning, circuit breaker policy or automated migration rollback unless source proves it.

### Task 4: Write Course and Student service architecture docs

**Files:**
- Create: \`docs/architecture/backend/course-service/architecture.md\`
- Create: \`docs/architecture/backend/student-service/architecture.md\`
- Read: \`docs/database/{course-service,student-service}/data-model.md\`
- Read: \`docs/api/{course-service,student-service}/**\`

- [ ] **Step 1: Write Course Service architecture**

Document ownership of Course, Lesson, Enrollment and LessonProgress. Explain Domain entities/rules, Application use cases/ports, Infrastructure EF Core and typed clients, and API endpoint/composition responsibilities. Add a Mermaid request flow and links to the Course data model and API docs.

- [ ] **Step 2: Write Student Service architecture**

Document ownership of Student/profile, its role as a query boundary, the four layers, database ownership and logical references. Add a Mermaid request flow and links to Student data model/API docs.

- [ ] **Step 3: Separate current and future behavior**

For both docs, make every capability source-backed or explicitly future; do not claim a direct cross-service database access or MinIO access.

### Task 5: Write Media, Notification and Scheduler service architecture docs

**Files:**
- Create: \`docs/architecture/backend/media-service/architecture.md\`
- Create: \`docs/architecture/backend/notification-service/architecture.md\`
- Create: \`docs/architecture/backend/scheduler-service/architecture.md\`
- Read: \`docs/database/{media-service,notification-service,scheduler-service}/data-model.md\`
- Read: \`docs/api/{media-service,notification-service,scheduler-service}/**\`
- Delete: \`docs/architecture/rich-content-and-media.md\`

- [ ] **Step 1: Write Media Service architecture**

Explain \`media_objects\`/\`media_usages\`, MinIO ownership, upload/content-stream flow, storage and URL abstractions, and API multipart boundary. Include a Mermaid flow and separate implemented lifecycle behavior from stale-object recovery or future changes.

- [ ] **Step 2: Write Notification Service architecture**

Explain notification/batch/item ownership, recipient and media boundaries, worker role when source-backed, outbox/idempotency constraints, and data model links. Include a Mermaid flow; mark unimplemented dispatch behavior as future.

- [ ] **Step 3: Write Scheduler Service architecture**

Explain Scheduler API, Domain/Application/Infrastructure and Worker host. State that \`background_jobs\` and \`background_job_runs\` are owned locally; mark Cron evaluation, polling, claiming, handler execution and target-service calls as future unless source evidence shows otherwise.

- [ ] **Step 4: Remove the superseded rich-content file**

Move its Media content into Media Service and its Notification distribution content into Notification Service before deletion.

### Task 6: Move Frontend documentation and repair all links

**Files:**
- Create: \`docs/architecture/frontend/architecture.md\`
- Delete: \`docs/architecture/frontend.md\`
- Modify: \`docs/README.md\`
- Modify: \`docs/development/setup.md\`
- Modify: \`docs/development/tech-stack.md\`

- [ ] **Step 1: Write Frontend architecture**

Preserve source-backed React, Redux Toolkit, Axios, hook/page/component, toast, CORS and Gateway documentation. Add a Mermaid page → hook → Redux → Axios → Gateway flow and link \`../backend/overview.md\`.

- [ ] **Step 2: Update every known inbound link**

Replace old \`docs/architecture/frontend.md\` links in \`docs/README.md\`, \`docs/development/setup.md\` and \`docs/development/tech-stack.md\`. Update all other architecture links to their new relative path.

- [ ] **Step 3: Remove the old frontend file**

Delete \`docs/architecture/frontend.md\` only after all link updates are complete.

### Task 7: Validate documentation integrity

**Files:**
- Verify: \`docs/architecture/**/*.md\`, \`docs/README.md\`, \`docs/development/*.md\`

- [ ] **Step 1: Detect obsolete paths**

Run:

\`\`\`bash
rg -n 'architecture/(overview|microservices|clean-architecture|frontend|service-communication|rich-content-and-media|message-contract-template|uml|conclusion)\\.md' docs --glob '*.md'
\`\`\`

Expected: no output.

- [ ] **Step 2: Check Markdown status sections and whitespace**

Run:

\`\`\`bash
rg -n '## Đã triển khai hiện tại|## Định hướng/chưa triển khai' docs/architecture/backend/*/architecture.md docs/architecture/backend/building-blocks/*.md
git diff --check
\`\`\`

Expected: each of five service docs and six BuildingBlocks docs contains both status sections; no whitespace errors.

- [ ] **Step 3: Review changed-file scope**

Run:

\`\`\`bash
git diff --name-only
\`\`\`

Expected: only \`docs/architecture/\`, inbound documentation links and approved design/plan artifacts change.

### Task 8: Expand service and BuildingBlocks details

**Files:**
- Modify: \`docs/architecture/backend/{course-service,student-service,media-service,notification-service,scheduler-service}/architecture.md\`
- Create: \`docs/architecture/backend/<service>/details/*.md\` only for actual Domain, Application, Infrastructure, API, Worker, database/integrations and test projects
- Rename: \`docs/architecture/backend/building-blocks/README.md\` to \`architecture.md\`
- Move: \`docs/architecture/backend/building-blocks/{contracts,presentation,http,messaging,database-migration,testing}.md\` to \`details/\`

- [ ] **Step 1: Link each overview to actual detail files**

Create a `details/` directory for every service. Keep `architecture.md` as the entry point and add only links whose backing project/runtime exists; Course and Student must not link a non-existent Worker document.

- [ ] **Step 2: Write layer detail documents from source boundaries**

For each existing project layer, document its project responsibility, allowed dependencies, primary source folders, runtime flow, usage/entry-point and troubleshooting. Each file includes Mermaid plus **Đã triển khai hiện tại** / **Định hướng/chưa triển khai**.

- [ ] **Step 3: Restructure BuildingBlocks details**

Make \`building-blocks/architecture.md\` the only overview. Move its six focused documents under \`building-blocks/details/\`, repair all relative links and preserve every diagram, usage snippet, troubleshooting table and current/future classification.

- [ ] **Step 4: Commit only when explicitly requested**

Do not commit or push in this task unless the user explicitly asks.

## Self-review

Spec coverage is complete: structure, shared Backend rules, five service docs, Frontend move, current/future separation, link repair and validation each have a dedicated task. The plan has no implementation placeholders and introduces no runtime change.
