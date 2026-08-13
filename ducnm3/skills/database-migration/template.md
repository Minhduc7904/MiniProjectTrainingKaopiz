# Database Migration Template

## Change record

```text
Owning service:
Database:
Current latest version:
New migration:
Reason:
Compatibility impact:
Backfill required:
Recovery/rollback approach:
Affected API/business flow:
```

## SQL skeleton

```sql
-- V###__short_description.sql
ALTER TABLE example_table
    ADD COLUMN example_field VARCHAR(100) NULL
        COMMENT 'Ý nghĩa nghiệp vụ và nullability';

CREATE INDEX ix_example_table_example_field
    ON example_table (example_field);
```

Không sao chép skeleton một cách máy móc. Chọn DDL, online strategy, default,
constraint và index theo schema thật.

## Execution checklist

```text
- [ ] Đã đọc migration skill/reference/template và database docs.
- [ ] Đã xác định service sở hữu schema.
- [ ] Không sửa migration đã có trong schema_migrations.
- [ ] Version và filename mới đúng quy ước.
- [ ] Field/index/constraint có tên và COMMENT rõ.
- [ ] Không chứa development seed data.
- [ ] Đã cập nhật docs/database.
- [ ] Đã apply migration thành công.
- [ ] Đã xác minh history/checksum và chạy lại migration runner.
- [ ] Đã scaffold đúng service.
- [ ] Đã review generated DbContext/entities.
- [ ] Integration test dùng Testcontainer đã pass.
- [ ] Build, formatter và git diff --check đã pass.
```

## Verification commands

```bash
dotnet tool restore
set -a
. ./.env
set +a
sh scripts/database/tools/migrate.sh <service>
sh scripts/database/tools/scaffold.sh <service>
dotnet build backend/Lms.sln --configuration Release
dotnet test backend/Services/<Service>/<Service>Service.IntegrationTests/<Service>Service.IntegrationTests.csproj
dotnet format backend/Lms.sln --verify-no-changes --no-restore
git diff --check
```

Chỉ chạy command phù hợp với service và môi trường hiện tại; không in
connection string hoặc credential vào log.
