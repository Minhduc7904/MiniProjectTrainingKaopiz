# Phase 2 — Project Planning

## Architecture Direction

```mermaid
flowchart LR
  Client --> Gateway[YARP Gateway]
  Gateway --> Course
  Gateway --> Student
  Gateway --> Media
  Gateway --> Notification
  Gateway --> Scheduler
  Media --> MinIO
  Media & Notification --> MQ[RabbitMQ]
  MQ --> MW[Media Worker]
  MQ --> NW[Notification Worker]
  Course --> CDB[(Course DB)]
  Student --> SDB[(Student DB)]
  Media --> MDB[(Media DB)]
  Notification --> NDB[(Notification DB)]
  Scheduler --> JDB[(Scheduler DB)]
```

Mỗi service giữ database riêng; HTTP dùng cho query cần response, RabbitMQ cho
command/event. Scheduler không sở hữu business data và job execution chưa có
evidence runtime. Xem [architecture](../../architecture/README.md).

## Service Responsibilities

| Service | Responsibility | Own DB | External dependency |
| --- | --- | --- | --- |
| Course | Course/Lesson/Enrollment/Progress boundary | Có | Student/Media chỉ logical reference. |
| Student | Student/profile query boundary | Có | Không cross-DB. |
| Media | Object metadata/usage và MinIO access | Có | MinIO, Student API, RabbitMQ. |
| Notification | Notification/batch/item | Có | Student API, Media messaging, RabbitMQ. |
| Scheduler | Background job/run data | Có | RabbitMQ host; execution Planned. |

## Tech Stack & Environment Strategy

| Category | Technology | Purpose | Status |
| --- | --- | --- | --- |
| Runtime | .NET / ASP.NET Core | API và Worker host | In use |
| Data | MySQL 8.4, EF Core/Pomelo | Database per service | In use |
| Storage | MinIO | Media object storage | In use, chỉ Media truy cập |
| Messaging | RabbitMQ, MassTransit | Command/event và Worker | In use |
| Gateway | YARP | Public HTTP entry point | In use |
| Container | Docker Compose | Local stack | In use |
| Testing | NUnit, TestServer, Testcontainers | Quality boundary | In use |
| Logging | Structured logging | Observability direction | Cần xác nhận implementation tập trung |

Local Development chạy infrastructure và service bằng Docker Compose. Khi benchmark,
giữ cùng máy, dataset, configuration; ghi warm-up, số lần chạy, thời gian, memory
và throughput. Không chạy benchmark trong Phase 0–2.

## Measurement Strategy

| Nhóm | Dataset/baseline | Metric/evidence bắt buộc | Trạng thái Phase 0–2 |
| --- | --- | --- | --- |
| Notification batch | 3k, 10k, 100k recipient; page/chunk | Execution time, peak memory, throughput, success/failure | Planned |
| CSV Course | 10k, 100k, 300k; SELECT ALL/build string so với batch/stream | Total time, peak memory, output size, TTFB nếu đo được | Planned |
| N+1 | Course → Lessons → Progress | Số SQL query và response time trước/sau | Planned |
| Index | 10k, 100k, 1M Course | Index, độ chọn lọc, rows examined, thời gian trước/sau | Planned |
| Query Plan | Cùng query/index experiment | `EXPLAIN`/`EXPLAIN ANALYZE`, access type, index được chọn, bottleneck | Planned |
| Pagination | Dataset lớn, offset tăng và cursor/keyset khi phù hợp | Page size, offset/cursor, response time | Planned |
| API | 10k, 100k record, cùng request trước/sau optimization | Response time, SQL query count, throughput/peak memory nếu đo được | Planned |

> [!IMPORTANT]
> `Planned` nghĩa là đã khóa đầu vào và cách đo, không phải benchmark đã chạy
> hoặc optimization đã hoàn tất.

## Delivery, Quality & Environment

- Local infrastructure chạy Docker Compose; API/Worker build bằng Dockerfile.
- Test strategy: NUnit unit/component/integration; Testcontainers khi cần dependency thật.
- Git strategy tham chiếu [branch strategy](../../development/branch-and-commit-strategy.md); `ducnm3` là integration branch, ticket branch dùng `feature/<ticket>`.
- Milestone: Phase 0–2 → Requirements → Design/Testcase → Development → Verify/Demo.
- Deliverable Phase 0–2: scope, direction, assumptions/risks, traceability, planning DoD.

## Milestone và gate

| Milestone | Output chính | Gate |
| --- | --- | --- |
| Phase 0–2 | Initiation, scope, architecture direction, registers, planning và lead traceability | G1 — Project Plan Approved |
| Phase 3 | Function List, functional specification, BR/AC/NFR và traceability | G2 — Requirements Approved |
| Phase 4 | Basic Design, API/database/messaging design và testcase | G3 — Design + Testcase Approved |
| Phase 5+ | Implementation, verification, benchmark và demo evidence | Definition of Done/acceptance |
