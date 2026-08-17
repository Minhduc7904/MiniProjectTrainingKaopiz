# Tổng quan kiến trúc Backend

Backend dùng ASP.NET Core microservices, YARP Gateway, MySQL, RabbitMQ và MinIO. Gateway là public entry point; client không gọi service nội bộ trực tiếp.

~~~mermaid
flowchart LR
  Client --> Gateway[YARP API Gateway]
  Gateway --> Course[Course Service]
  Gateway --> Student[Student Service]
  Gateway --> Media[Media Service]
  Gateway --> Notification[Notification Service]
  Gateway --> Scheduler[Scheduler Service]
  Course --> CourseDb[(lms_course_db)]
  Student --> StudentDb[(lms_student_db)]
  Media --> MediaDb[(lms_media_db)]
  Media --> MinIO[(MinIO)]
  Notification --> NotificationDb[(lms_notification_db)]
  Scheduler --> SchedulerDb[(lms_scheduler_db)]
  Course --- MQ[RabbitMQ]
  Student --- MQ
  Media --- MQ
  Notification --- MQ
  Scheduler --- MQ
~~~

## Cấu trúc thư mục tổng quan

```text
backend/
├── BuildingBlocks/                 # Contract và adapter kỹ thuật dùng chung
│   ├── BuildingBlocks.Contracts/
│   ├── BuildingBlocks.DatabaseMigration/
│   ├── BuildingBlocks.Http/
│   ├── BuildingBlocks.Messaging.Abstractions/
│   ├── BuildingBlocks.Messaging/
│   ├── BuildingBlocks.Presentation/
│   ├── BuildingBlocks.Shared/
│   └── *.Tests/                    # Unit/integration test cho shared boundary
├── Gateway/
│   └── Lms.ApiGateway/             # YARP public entry point
├── Services/
│   ├── Course/                     # Course, Lesson, Enrollment, Progress
│   ├── Student/                    # Student query boundary
│   ├── Media/                      # Media API/Worker và MinIO ownership
│   ├── Notification/               # Notification API/Worker và batch
│   └── Scheduler/                  # Job definition/run và Worker host
├── Tools/
│   ├── Lms.DataSeeder/
│   ├── Lms.DataSeeder.UnitTests/
│   └── Lms.DataSeeder.IntegrationTests/
├── Directory.Build.props           # Cấu hình build dùng chung
├── Dockerfile                      # Build image backend theo target project
└── Lms.sln                         # Solution tổng hợp
```

Mỗi thư mục dưới `Services/` được tổ chức theo Clean Architecture. Không đưa
`bin/` và `obj/` vào cây vì đây là build artifact được sinh ra, không phải cấu
trúc source. Xem cây chi tiết tại tài liệu của từng service và
[BuildingBlocks](building-blocks/architecture.md).

## Ownership

Mỗi service sở hữu database riêng. ID của service khác là logical reference; không có foreign key hoặc truy vấn chéo database. Chỉ [Media Service](media-service/architecture.md) sở hữu MinIO.

| Service | Ownership |
| --- | --- |
| Course | Course, Lesson, Enrollment, LessonProgress |
| Student | Student và profile |
| Media | Metadata, usage, upload/content và MinIO |
| Notification | Notification đơn, batch, batch item, inbox |
| Scheduler | Định nghĩa job và lịch sử job run |

## Đã triển khai hiện tại

Gateway có reverse proxy, CORS, health/info endpoint và Swagger aggregation. Mỗi service API chạy SQL migration khi khởi động, có health endpoint và cấu hình MassTransit. Media và Notification có Worker riêng.

## Định hướng/chưa triển khai

Scheduler chưa có API nghiệp vụ, Cron evaluation, polling/claiming hoặc job handler.
