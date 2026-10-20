# Release Reference

## Verification baseline

```bash
dotnet build backend/Lms.sln --configuration Release
dotnet test backend/Lms.sln --configuration Release --no-build
dotnet format backend/Lms.sln --verify-no-changes --no-restore
docker compose --env-file .env.example config --quiet
git diff --check
```

Chạy thêm frontend, end-to-end hoặc migration smoke test khi release chứa các
thành phần đó.

## Safety

- Không tạo tag, push, merge hoặc deploy nếu user chưa yêu cầu.
- Không dùng `--no-verify`, force push hoặc bỏ qua failed check.
- Không in secret/connection string vào log.
- Review migration history/checksum trước deployment.
- Xác định health endpoint và dependency readiness.

## Documentation

Cập nhật runbook khi deployment order, rollback, monitoring, migration hoặc
configuration thay đổi.
