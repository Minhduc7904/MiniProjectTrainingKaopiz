# Integration test Student Service

## Phạm vi

Dự án:
`backend/Services/Student/StudentService.IntegrationTests/StudentService.IntegrationTests.csproj`

Mã nguồn:
`Persistence/StudentListRepositoryIntegrationTests.cs`

Dependency: `mysql:8.4` qua Testcontainers. Fixture áp dụng các SQL migration
production từ `StudentService.Infrastructure/Database/Migrations`.

Chạy:

```bash
docker info
dotnet test backend/Services/Student/StudentService.IntegrationTests/StudentService.IntegrationTests.csproj
```

## Vòng đời và isolation

1. Fixture tạo MySQL container với database/credential chỉ dành cho test.
2. `SqlMigrationRunner` áp dụng migration thật.
3. Test xóa row thuộc container riêng và thêm dữ liệu UUID/timestamp cố định.
4. Repository production query bằng `StudentDbContext`.
5. Fixture dispose container trong `OneTimeTearDown`.

Không sử dụng MySQL Docker Compose hoặc dữ liệu seed của developer.

## Ca kiểm thử

- `CountAsync_AllStudentStatuses_ReturnsEveryRow`: thêm Student ACTIVE và BLOCKED vào MySQL Testcontainer; đạt khi repository production đếm cả hai row.

- `StatusAndDuplicateSortValuesReturnStablePages`: tạo ba Học viên `ACTIVE`,
  trong đó hai row có cùng `created_at`, và một Học viên `BLOCKED`; query hai
  page theo `createdAt desc`.
- Đạt khi:
  - filter chỉ đếm ba row `ACTIVE`;
  - `totalPages` bằng `2` với pageSize `2`;
  - hai row cùng timestamp được sắp ổn định bằng `id DESC`;
  - page thứ hai không lặp item từ page thứ nhất.
