# Kiểm thử tích hợp Notification Service

## Phạm vi

Dự án: `backend/Services/Notification/NotificationService.IntegrationTests/NotificationService.IntegrationTests.csproj`.

Mã nguồn: `Persistence/NotificationBatchLeaseIntegrationTests.cs`.

Dependency: MySQL `8.4` do Testcontainers tạo riêng; không dùng MySQL Docker
Compose của developer. Fixture áp dụng toàn bộ migration SQL production, gồm
`V003__add_batch_item_claim_lease.sql`, bằng `SqlMigrationRunner`.

Chạy:

```bash
dotnet test backend/Services/Notification/NotificationService.IntegrationTests/NotificationService.IntegrationTests.csproj
```

## Ca kiểm thử

| Kiểm thử | Thiết lập và thao tác | Đạt khi |
| --- | --- | --- |
| `ClaimChunkAsync_ConcurrentWorkers_ReservesDisjointItemsAndReclaimsExpiredLease` | Hai repository dùng DbContext riêng claim cùng batch; một item `PROCESSING` có lease hết hạn và một item `PENDING`. | Mỗi worker nhận một item khác nhau; cả hai có lease mới chưa hết hạn, chứng minh migration/index và claim `SKIP LOCKED` hoạt động. |
