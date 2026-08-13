# Database Migration Reference

## Source of truth

- SQL migration là nguồn chuẩn của schema.
- EF Core scaffold chỉ phản ánh schema đã apply; không dùng
  `dotnet ef migrations add`, `Database.Migrate()` hoặc generated model làm
  nguồn thiết kế.
- Mỗi service sở hữu database, migration folder, connection string và scaffold
  riêng. Không tạo foreign key xuyên database.

## Vị trí migration

```text
backend/Services/<Service>/<Service>Service.Infrastructure/
└── Database/Migrations/V###__description.sql
```

Service key dùng cho scripts:

- `course`
- `student`
- `media`
- `notification`
- `scheduler`

## Version và history

1. Liệt kê migration của service và chọn số kế tiếp liên tục.
2. Kiểm tra `schema_migrations` trước khi thay đổi.
3. Nếu version đã có trong history, tuyệt đối không sửa file tương ứng.
4. Checksum mismatch phải được điều tra; không xóa history để che lỗi.
5. Chỉ reset history khi có kế hoạch reset baseline được phê duyệt rõ ràng.

## Thiết kế SQL

- Dùng tên constraint/index có ý nghĩa: `pk_`, `fk_`, `uq_`, `ix_`, `chk_`.
- Field mới phải có kiểu, nullability, default và `COMMENT` rõ ràng.
- Đánh giá lock/table rewrite đối với table lớn.
- Backfill phải deterministic, giới hạn phạm vi và có recovery plan.
- Không dùng dữ liệu seed development làm migration.
- Cross-service ID là logical reference, không phải foreign key.

## Apply migration

```bash
set -a
. ./.env
set +a
sh scripts/database/tools/migrate.sh <service>
```

Runner phải:

- chờ MySQL khả dụng trong container workflow;
- tạo/đọc `schema_migrations`;
- kiểm tra checksum;
- apply đúng thứ tự;
- từ chối history không nhất quán.

## Scaffold EF Core

```bash
dotnet tool restore
set -a
. ./.env
set +a
sh scripts/database/tools/scaffold.sh <service>
```

Sau scaffold:

1. Review entity nullability, numeric type, generated column và navigation.
2. Review index/constraint mapping trong `DbContext`.
3. Không scaffold `schema_migrations`.
4. Không để connection string trong generated code.
5. Không tự ý chỉnh generated mapping để khác schema; sửa migration rồi
   scaffold lại.

## Testing

Đọc `skills/test-integration/` trước khi viết migration test.

Integration fixture phải:

- tạo MySQL Testcontainer riêng;
- apply toàn bộ migration của service;
- xác minh history/checksum;
- chạy behavior sử dụng schema mới;
- không kết nối database Docker Compose của developer.

## Documentation

Cập nhật:

- `docs/database/` cho field, index, constraint và data rule;
- `docs/runbooks/` nếu cần backfill/recovery/rollback;
- `docs/tests/<service>/integration.md` cho migration test;
- API/business flow nếu schema làm thay đổi behavior công khai.
