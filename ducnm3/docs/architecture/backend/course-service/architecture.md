# Kiến trúc Course Service

## Mục đích và ownership

Service sở hữu Course, Lesson, Enrollment, LessonProgress và Markdown Course/Lesson. Xem [data model](../../../database/course-service/data-model.md). Student ID và media URL là logical reference; service không truy cập Student database hoặc MinIO.

~~~mermaid
flowchart LR
  Gateway --> Api[Course API] --> App[Application] --> Domain
  App --> Infra[Infrastructure] --> Db[(Course database)]
~~~

## Các tầng

- **Domain:** entity/rule Course và Lesson.
- **Application:** use case, validation và persistence/client abstraction.
- **Infrastructure:** SQL migration, persistence và health probe.
- **API:** migration startup, health/info và HTTP mapping.

## Tài liệu chi tiết

- [Domain](details/domain.md)
- [Application](details/application.md)
- [Infrastructure](details/infrastructure.md)
- [API](details/api.md)
- [Database và integration](details/database-integrations.md)
- [Testing](details/testing.md)

## Đã triển khai hiện tại

API chạy migration, service info và database health; schema/migration tồn tại. [API docs](../../../api/course-service/README.md) là contract cần đối chiếu endpoint mapping trước khi coi là runtime behavior.

## Định hướng/chưa triển khai

Không coi Course CRUD, export, pagination hay Media/Student integration là đã chạy nếu thiếu endpoint/Application implementation.
