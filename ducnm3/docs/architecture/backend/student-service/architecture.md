# Kiến trúc Student Service

## Mục đích và ownership

Student Service sở hữu Student/profile trong [data model](../../../database/student-service/data-model.md) và là query boundary cho service cần tra cứu học viên.

~~~mermaid
flowchart LR
  Gateway --> Api[Student API] --> App[Application] --> Domain
  App --> Infra[Infrastructure] --> Db[(Student database)]
~~~

## Cấu trúc thư mục

```text
backend/Services/Student/
├── StudentService.Api/
│   └── Endpoints/                  # GET list/detail và health mapping
├── StudentService.Application/
│   └── Features/
│       └── Students/
│           ├── GetById/
│           └── GetList/
├── StudentService.Domain/          # Student model và trạng thái
├── StudentService.Infrastructure/
│   ├── Database/
│   │   └── Migrations/
│   ├── Health/
│   └── Persistence/
│       └── Scaffolded/
├── StudentService.UnitTests/
├── StudentService.ComponentTests/
│   └── Endpoints/
└── StudentService.IntegrationTests/
    └── Persistence/
```

Feature folder nằm trong Application; API chỉ bind/map request, còn
Infrastructure implement persistence và database boundary.

## Các tầng

- **Domain:** model Student và trạng thái.
- **Application:** list/detail query và persistence abstraction.
- **Infrastructure:** EF Core, migration, health probe.
- **API:** endpoint list/detail, health/info và response envelope.

## Tài liệu chi tiết

- [Domain](details/domain.md)
- [Application](details/application.md)
- [Infrastructure](details/infrastructure.md)
- [API](details/api.md)
- [Database và integration](details/database-integrations.md)
- [Testing](details/testing.md)

## Đã triển khai hiện tại

API map list và detail Student; Application/Infrastructure/EF Core và component/integration tests cùng tồn tại. Xem [API docs](../../../api/student-service/README.md).

## Định hướng/chưa triển khai

Không có foreign key hoặc database access liên service; authentication/authorization phụ thuộc use case thực tế.
