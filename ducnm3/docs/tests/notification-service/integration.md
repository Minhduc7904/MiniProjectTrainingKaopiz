# Kiểm thử tích hợp Notification Service

## Phạm vi

Dự án: `backend/Services/Notification/NotificationService.IntegrationTests/NotificationService.IntegrationTests.csproj`.

Mã nguồn: `Persistence/NotificationBatchLeaseIntegrationTests.cs` và
`Persistence/NotificationBatchSnapshotIntegrationTests.cs`.

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
| `CompleteClaimAsync_ClaimedItem_StoresNotificationAndUpdatesCounters` | Một item `PROCESSING` có lease token hợp lệ được complete với kết quả gửi thành công. | Repository lọc item theo batch và lease token, tạo notification, xóa lease và tăng counter thành công mà không dùng `Guid[]` trong LINQ query. |
| `AppendSnapshotPageAsync_RedeliveredRecipients_DoesNotDuplicateItems` | Repository production ghi cùng trang recipient hai lần vào MySQL 8.4 Testcontainer sau khi áp dụng migration thật. | Lần đầu thêm hai item `PENDING`; redelivery thêm 0 item và database vẫn có đúng hai recipient, chứng minh snapshot idempotent và `requestedCount` không bị trừ sai. |
