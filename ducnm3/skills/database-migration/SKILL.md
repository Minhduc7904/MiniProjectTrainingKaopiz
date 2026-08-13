---
name: database-migration
description: Triển khai SQL migration Database First và scaffold EF Core cho từng microservice. Bắt buộc dùng khi thay đổi table, column, index, constraint, generated column, dữ liệu backfill hoặc scaffold DbContext/entity.
---

# Database Migration

## Bắt buộc đọc trước

1. `rules/database-migrations.md`.
2. `rules/testing.md`.
3. `docs/guide/MIGRATION_GUIDE.md`.
4. Database document của service trong `docs/database/`.
5. [`reference.md`](reference.md) và [`template.md`](template.md).

Không sửa schema, migration hoặc generated EF code trước khi đọc đủ các file
trên.

## Workflow

1. Xác định microservice sở hữu schema và connection string của chính service đó.
2. Kiểm tra migration hiện có và `schema_migrations`; không sửa migration đã áp
   dụng.
3. Chọn version kế tiếp và tạo
   `V###__lowercase_description.sql` trong Infrastructure của service.
4. Viết forward-only SQL an toàn, đặt tên rõ cho constraint/index và thêm
   `COMMENT` cho field mới.
5. Không thêm development seed data vào migration.
6. Cập nhật database docs, compatibility impact và rollback/recovery procedure.
7. Apply migration bằng `scripts/database/tools/migrate.sh <service>`.
8. Xác minh version/checksum trong `schema_migrations` và chạy lại để kiểm tra
   idempotent history.
9. Chạy `scripts/database/tools/scaffold.sh <service>` sau khi migration thành
   công.
10. Review generated `DbContext`/entities; chỉ giữ mapping thuộc database của
    service.
11. Thêm integration test áp dụng migration thật và kiểm tra constraint/index
    hoặc repository behavior bị ảnh hưởng.
12. Chạy build, test, formatter và `git diff --check`.

## Definition of done

- Migration cũ không bị thay đổi checksum.
- Migration mới apply thành công trên database sạch và database đã có history.
- EF scaffold khớp schema mới.
- Reset development database vẫn xóa toàn bộ dữ liệu do schema sở hữu.
- Tests và docs database/runbook đã đồng bộ.
