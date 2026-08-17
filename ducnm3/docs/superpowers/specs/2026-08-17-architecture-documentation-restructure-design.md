# Thiết kế tái cấu trúc tài liệu kiến trúc

## Mục tiêu và phạm vi

Tổ chức lại `docs/architecture/` theo Backend và Frontend, đồng thời tạo một
tài liệu kiến trúc độc lập cho Course, Student, Media, Notification và Scheduler
Service. Mỗi tài liệu phải phân biệt rõ phần đã triển khai với định hướng chưa
triển khai. Phạm vi chỉ gồm Markdown và liên kết Markdown; không thay đổi source
code, API contract, migration, Docker Compose hoặc runtime.

## Cấu trúc đích

```text
docs/architecture/
├── README.md
├── backend/
│   ├── overview.md
│   ├── uml.md
│   ├── shared/
│   │   ├── clean-architecture.md
│   │   ├── service-communication.md
│   │   └── message-contract-template.md
│   ├── building-blocks/
│   │   ├── architecture.md
│   │   └── details/{contracts,presentation,http,messaging,database-migration,testing}.md
│   ├── course-service/{architecture.md,details/}
│   ├── student-service/{architecture.md,details/}
│   ├── media-service/{architecture.md,details/}
│   ├── notification-service/{architecture.md,details/}
│   └── scheduler-service/{architecture.md,details/}
└── frontend/
    └── architecture.md
```

`docs/architecture/README.md` là mục lục duy nhất. Các file cũ sẽ được gỡ sau
khi toàn bộ link từ `docs/README.md`, `docs/development/` và tài liệu kiến trúc
được cập nhật; không giữ hai nguồn sự thật.

## Phân loại nội dung

| Nguồn hiện tại | Đích |
| --- | --- |
| `overview.md`, `microservices.md`, `conclusion.md` | `backend/overview.md` |
| `clean-architecture.md` | `backend/shared/clean-architecture.md` |
| `service-communication.md` | `backend/shared/service-communication.md` |
| `message-contract-template.md` | `backend/shared/message-contract-template.md` |
| `rich-content-and-media.md` | `backend/media-service/architecture.md` và `backend/notification-service/architecture.md` |
| `uml.md` | `backend/uml.md` |
| `frontend.md` | `frontend/architecture.md` |

`backend/overview.md` mô tả Client, YARP Gateway, RabbitMQ, MinIO, năm service
và database ownership. Tài liệu khẳng định Gateway là public entry point, chỉ
Media Service truy cập MinIO và không service nào truy vấn chéo database.

## BuildingBlocks

`backend/building-blocks/` là tài liệu riêng cho các thư viện dùng chung, không
phải một service và không sở hữu database nghiệp vụ. `architecture.md` mô tả
dependency graph; sáu tài liệu trong `details/` tách theo trách nhiệm để người đọc
có thể tra cứu độc lập:

| Tài liệu | Project/source được mô tả | Trách nhiệm |
| --- | --- | --- |
| `contracts.md` | `BuildingBlocks.Contracts` | API/health contracts, route, header và service constants. |
| `presentation.md` | `BuildingBlocks.Presentation` | Middleware, response envelope, CORS, info/health endpoint, Gateway Swagger. |
| `http.md` | `BuildingBlocks.Http` | Typed `HttpClient`, truyền correlation ID, timeout và resilience. |
| `messaging.md` | `BuildingBlocks.Messaging.Abstractions`, `BuildingBlocks.Messaging` | Command/event abstraction và MassTransit/RabbitMQ adapter, consumer, retry, health probe. |
| `database-migration.md` | `BuildingBlocks.DatabaseMigration` | SQL migration runner MySQL, lock, checksum và history table. |
| `testing.md` | Ba test project BuildingBlocks | Unit, TestServer component và RabbitMQ Testcontainers coverage. |

Mỗi tài liệu BuildingBlocks có mục đích, sơ đồ Mermaid, usage bằng API signature
hoặc integration snippet, troubleshooting, dependency direction và hai phần
**Đã triển khai hiện tại** / **Định hướng/chưa triển khai**. Các docs khẳng định
`Messaging.Abstractions` không phụ thuộc MassTransit; chỉ `Messaging` là adapter
transport. Các test project là test boundary, không phải thư viện runtime.

## Chuẩn Backend dùng chung

`backend/shared/clean-architecture.md` xác định dependency rule:

```text
API / Worker → Application → Domain
Infrastructure → Application
Domain → không phụ thuộc layer nào
```

Infrastructure chứa EF Core/MySQL, MinIO, HTTP client và MassTransit adapter;
API hoặc Worker là composition root, transport và middleware.

`backend/shared/service-communication.md` quy định HTTP cho QUERY cần response,
RabbitMQ `Send` cho COMMAND có một owner và RabbitMQ `Publish` cho EVENT có nhiều
subscriber. At-least-once delivery đòi hỏi idempotency; Outbox/Inbox là điều
kiện trước khi dùng messaging cho luồng nghiệp vụ quan trọng.

## Chuẩn tài liệu năm service

Mỗi file `architecture.md` gồm bảy phần:

1. Mục đích và ownership.
2. Thành phần và dependency.
3. Luồng xử lý Mermaid từ Gateway/consumer qua các layer đến dependency.
4. Giải thích Domain, Application, Infrastructure và API/Worker.
5. Database và integration boundary, có link đến data model.
6. Đã triển khai hiện tại, chỉ ghi điều có bằng chứng trong source/API/migration.
7. Định hướng/chưa triển khai, không mô tả như behavior đang chạy.

Mỗi `architecture.md` là entry point và link đến `details/`. Detail được tách theo
project/layer thực tế: `domain.md`, `application.md`, `infrastructure.md`, `api.md`,
`worker.md`, `database-integrations.md` và `testing.md`. Không tạo file trống:
Course/Student không có Worker project nên không có `worker.md`; service chỉ có
tài liệu cho layer hoặc runtime tồn tại trong source. Mỗi detail có purpose,
Mermaid, dependency boundary, usage hoặc entry point, troubleshooting và trạng
thái hiện tại/định hướng.

| Service | Ownership chính | Boundary đặc biệt |
| --- | --- | --- |
| Course | Course, Lesson, Enrollment, LessonProgress, Markdown Course/Lesson | Logical reference Student/Media; không truy cập MinIO hoặc database service khác. |
| Student | Student và profile | Nguồn query/actor verification; không có foreign key liên service. |
| Media | `media_objects`, `media_usages`, upload/content | Chỉ service này dùng MinIO; Application dùng storage/URL abstraction. |
| Notification | Notification đơn, batch, batch item, inbox | Lấy recipient qua contract; media usage qua messaging boundary. |
| Scheduler | `background_jobs`, `background_job_runs`, API, Worker host | Ghi rõ CRON, polling, claiming và handler là định hướng nếu source chưa có. |

## Frontend

`frontend/architecture.md` mô tả React SPA qua YARP Gateway, Redux Toolkit state
boundary, Axios interceptor, hook/page/component boundary, Workbench, CORS và
environment variables. Nó không lặp chi tiết Backend, chỉ liên kết đến
`../backend/overview.md` khi giải thích Gateway.

## Rà soát và xác minh

Nội dung cuối phải được đối chiếu với project layout, project reference,
`Program.cs`, `docs/api/`, `docs/database/`, Docker Compose, Gateway, frontend
source và business flow. Tuyên bố không có bằng chứng bị xóa hoặc chuyển sang
**Định hướng/chưa triển khai**.

Xác minh gồm:

1. `rg` không còn link tới đường dẫn kiến trúc cũ.
2. Toàn bộ link Markdown nội bộ còn hợp lệ.
3. Tên service, database, layer và BuildingBlocks dependency khớp repository.
4. `git diff --check` không báo whitespace.

## Ngoài phạm vi

- Đổi source code hoặc project structure.
- Tạo migration, endpoint hoặc message contract mới.
- Thay đổi runtime chỉ vì tài liệu phát hiện capability chưa triển khai.
