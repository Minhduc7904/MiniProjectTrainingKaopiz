# Foundation Tasks — Phase 5

## P5-01 — Chuẩn hóa Docker Compose và runtime baseline

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 5 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Requirement | Docker, NFR-ENV-01 |
| Dependency | Không |

**Mục tiêu:** Một lệnh `docker compose up` dựng được Gateway, năm API service,
các Worker, MySQL, MinIO và RabbitMQ với health check và cấu hình development
nhất quán.

**Phạm vi code/config:** `docker-compose.yml`, `backend/Dockerfile`, appsettings
của Gateway/service/worker, migration/bootstrap và MinIO provisioning trong
`scripts/`.

**Công việc:**

- Đối chiếu service, network, volume, environment, dependency và health check
  với `docs/development/docker.md`.
- Bảo đảm startup mới không phụ thuộc dữ liệu/volume còn sót; migration chạy
  đúng thứ tự và failure hiển thị đủ để chẩn đoán.
- Kiểm tra Gateway route tới từng API; Worker kết nối RabbitMQ; Media kết nối
  MinIO; từng service chỉ kết nối database thuộc ownership.
- Bổ sung smoke test/script cần thiết và cập nhật hướng dẫn chạy.

**Evidence bắt buộc:** log `docker compose up`, trạng thái container/health,
smoke request qua Gateway và kết quả migration trên database sạch.

## P5-02 — Chuẩn hóa error, identity, security và observability

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 6 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Requirement | NFR-ERROR-01, NFR-SEC-01, NFR-OBS-01 |
| Dependency | P5-01 |

**Mục tiêu:** Tạo cross-cutting baseline dùng chung trước khi mở rộng endpoint,
để validation, identity, authorization, error envelope và correlation không bị
mỗi service tự triển khai khác nhau.

**Phạm vi code:** `backend/BuildingBlocks/BuildingBlocks.Contracts/Api/`,
`backend/BuildingBlocks/BuildingBlocks.Presentation/`,
`backend/BuildingBlocks/BuildingBlocks.Http/`, Gateway và `Program.cs` của các
service.

**Công việc:**

- Chốt và triển khai development identity boundary đã được duyệt; không lấy
  actor/role đáng tin cậy từ payload nghiệp vụ.
- Chuẩn hóa `400/401/403/404/409/5xx`, error code ổn định và không lộ SQL,
  stack trace, credential, bucket hoặc object key.
- Forward/generate `X-Correlation-Id` qua Gateway, HTTP client và message log;
  log operation, message type và attempt nhưng không log secret.
- Bổ sung unit/component test cho middleware, identity/authorization và error
  mapping; cập nhật shared API docs.

**Evidence bắt buộc:** test middleware/component pass và sample response/log
cho validation, unauthorized, forbidden, not found, conflict và dependency
failure.
