# Tài liệu kiểm thử

Tài liệu này mô tả các kiểm thử đang được triển khai, theo cùng cách tổ chức với
tài liệu API:

```text
docs/tests/
├── shared-presentation/
│   ├── unit.md
│   └── component.md
├── api-gateway/
│   └── component.md
├── data-seeder/
│   ├── unit.md
│   └── integration.md
├── communication/
│   ├── unit.md
│   └── integration.md
├── <service>/
    ├── unit.md
    ├── component.md
    └── integration.md
└── scheduler-service/
    └── unit.md
```

Mỗi tài liệu chỉ liệt kê kiểm thử hiện có trong mã nguồn, mục đích, dữ liệu hoặc
thành phần phụ thuộc được dùng và điều kiện đạt. Không suy diễn rằng loại kiểm thử chưa có
đã được triển khai.

## Chạy kiểm thử

```bash
dotnet test backend/Lms.sln -m:1
```

Chạy một dự án cụ thể theo đường dẫn được ghi trong tài liệu của dịch vụ. Kiểm
thử tích hợp dùng Testcontainers cần Docker Engine đang chạy. Xem
[`../development/testing.md`](../development/testing.md) để biết quy ước chung
và cách thêm kiểm thử mới.

Danh mục Data Seeder:

- [`data-seeder/unit.md`](data-seeder/unit.md)
- [`data-seeder/integration.md`](data-seeder/integration.md)

Danh mục communication foundation:

- [`communication/unit.md`](communication/unit.md)
- [`communication/integration.md`](communication/integration.md)

Danh mục Media Service:

- [`media-service/unit.md`](media-service/unit.md): storage validation, trình tự
  `PENDING -> READY`, compensation sang `FAILED`, actor/owner.
- [`media-service/component.md`](media-service/component.md): health/upload/usage legacy và
  HTTP contract của URL usage bằng `MediaService.ComponentTests`.
- [`media-service/integration.md`](media-service/integration.md): MinIO storage,
  checksum SHA-256, migration/MySQL và active avatar uniqueness.

Danh mục Student Service:

- [`student-service/unit.md`](student-service/unit.md): lookup Học viên thành
  công, `STUDENT_NOT_FOUND` và validation/orchestration cho GET list.
- [`student-service/component.md`](student-service/component.md): HTTP binding,
  response envelope và offset pagination metadata cho GET list.
- [`student-service/integration.md`](student-service/integration.md): MySQL
  filter, stable ordering và page boundaries với migration production.

Danh mục Notification Service:

- [`notification-service/unit.md`](notification-service/unit.md): batch create,
  snapshot, dispatch, retry và fan-out media trong memory.
- [`notification-service/integration.md`](notification-service/integration.md):
  migration Notification production và claim lease với MySQL Testcontainer.

Coverage hiện tại của Media Upload/Usage tập trung ở unit và integration.
Gateway public paths, HTTP request binding/response envelope của hai command,
typed HTTP call tới Student Service và Scheduler cleanup cho `PENDING` stale
chưa có kiểm thử component/liên service.
